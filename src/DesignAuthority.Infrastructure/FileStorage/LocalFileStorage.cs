using DesignAuthority.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace DesignAuthority.Infrastructure.FileStorage;

public sealed class LocalFileStorage : IFileStorage
{
    private const long MaximumMinuteSize = 5 * 1024 * 1024;
    private readonly string rootPath;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        var configuredPath = options.Value.RootPath;
        if (string.IsNullOrWhiteSpace(configuredPath))
            throw new InvalidOperationException("FileStorage:RootPath must be configured outside source control.");

        rootPath = Path.GetFullPath(configuredPath);
        Directory.CreateDirectory(rootPath);
    }

    public async Task<StoredFile> StageAsync(
        string originalFileName,
        Stream content,
        long length,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (length < 0 || length > MaximumMinuteSize)
            throw new ArgumentOutOfRangeException(nameof(length), "Minuta must not exceed 5 MB.");
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name is required.", nameof(originalFileName));

        var safeOriginalName = Path.GetFileName(originalFileName.Replace('\\', '/'));
        if (!string.Equals(Path.GetExtension(safeOriginalName), ".docx", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only .docx Minutas are accepted.", nameof(originalFileName));

        var finalRelativePath = $"minutes/{DateTime.UtcNow:yyyy}/{Guid.NewGuid():N}.docx";
        var stagingRelativePath = finalRelativePath + ".staging";
        var stagingPath = ResolvePath(stagingRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(stagingPath)!);

        long copied = 0;
        try
        {
            await using var destination = new FileStream(
                stagingPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
            var buffer = new byte[81920];
            int read;
            while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
            {
                copied += read;
                if (copied > MaximumMinuteSize)
                    throw new ArgumentOutOfRangeException(nameof(length), "Minuta must not exceed 5 MB.");
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
            if (copied != length)
                throw new IOException("The uploaded Minuta length did not match its declared size.");
        }
        catch (Exception stageError)
        {
            try
            {
                if (File.Exists(stagingPath)) File.Delete(stagingPath);
            }
            catch (Exception cleanupError)
            {
                throw new AggregateException("Minuta staging failed and the partial file could not be cleaned up.",
                    stageError, cleanupError);
            }
            throw;
        }

        return new StoredFile(safeOriginalName, Path.GetFileName(finalRelativePath), finalRelativePath, copied);
    }

    public Task PromoteAsync(StoredFile file, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stagingPath = ResolvePath(file.RelativePath + ".staging");
        var finalPath = ResolvePath(file.RelativePath);
        if (File.Exists(finalPath)) throw new IOException("The generated Minuta destination already exists.");
        File.Move(stagingPath, finalPath);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(
            ResolvePath(relativePath), FileMode.Open, FileAccess.Read, FileShare.Read,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var finalPath = ResolvePath(relativePath);
        if (File.Exists(finalPath)) File.Delete(finalPath);
        var stagingPath = ResolvePath(relativePath + ".staging");
        if (File.Exists(stagingPath)) File.Delete(stagingPath);
        return Task.CompletedTask;
    }

    private string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new ArgumentException("A relative file reference is required.", nameof(relativePath));

        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var resolved = Path.GetFullPath(Path.Combine(rootPath, normalized));
        var rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;
        if (!resolved.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("File reference escapes the configured storage root.", nameof(relativePath));
        return resolved;
    }
}
