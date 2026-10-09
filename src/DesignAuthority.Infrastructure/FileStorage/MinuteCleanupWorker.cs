using Dapper;
using DesignAuthority.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DesignAuthority.Infrastructure.FileStorage;

public sealed class MinuteCleanupWorker(
    SqlConnectionFactory connections,
    LocalFileStorage storage,
    ILogger<MinuteCleanupWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Minuta cleanup batch failed; it will be retried on the next interval.");
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var pending = await connection.QueryAsync<CleanupItem>(new CommandDefinition("""
            SELECT TOP (20) FileCleanupId, RelativePath
            FROM dbo.FileCleanupQueue
            ORDER BY QueuedAtUtc, FileCleanupId;
            """, cancellationToken: cancellationToken));
        foreach (var item in pending)
        {
            try
            {
                await storage.DeleteAsync(item.RelativePath, cancellationToken);
                await connection.ExecuteAsync(new CommandDefinition(
                    "DELETE dbo.FileCleanupQueue WHERE FileCleanupId = @Id;",
                    new { Id = item.FileCleanupId }, cancellationToken: cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Minuta cleanup attempt failed for queue item {CleanupId}.", item.FileCleanupId);
                await connection.ExecuteAsync(new CommandDefinition("""
                    UPDATE dbo.FileCleanupQueue
                    SET AttemptCount = AttemptCount + 1, LastAttemptAtUtc = SYSUTCDATETIME(),
                        LastError = LEFT(@Error, 1000)
                    WHERE FileCleanupId = @Id;
                    """, new { Id = item.FileCleanupId, Error = exception.GetType().Name + ": " + exception.Message },
                    cancellationToken: cancellationToken));
            }
        }
    }

    private sealed record CleanupItem(long FileCleanupId, string RelativePath);
}
