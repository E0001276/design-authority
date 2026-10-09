using Dapper;
using DesignAuthority.Application.Abstractions;
using DesignAuthority.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DesignAuthority.Infrastructure.Repositories;

public sealed class WorkTrackingRepository(
    SqlConnectionFactory connections,
    ILogger<WorkTrackingRepository> logger) : IWorkTrackingRepository
{
    public async Task<PagedResult<WorkItemListRow>> ListAsync(
        WorkItemKind kind,
        WorkItemQuery query,
        CancellationToken cancellationToken)
    {
        var definition = GetListDefinition(kind);
        var sortColumn = definition.SortColumns.TryGetValue(query.SortBy, out var allowedSort)
            ? allowedSort
            : throw new ArgumentException("Unsupported sort column.", nameof(query));
        var direction = query.Descending ? "DESC" : "ASC";
        var pendingFilter = query.StatusId is null && kind is WorkItemKind.Finding or WorkItemKind.Agreement
            ? " AND status.Code IN ('OPEN', 'IN_PROGRESS')"
            : string.Empty;
        var statusFilter = query.StatusId is null ? string.Empty : " AND w.StatusId = @StatusId";
        var technologyFilter = query.TechnologyId is null ? string.Empty : " AND w.TeamTechnologyId = @TechnologyId";
        var searchFilter = string.IsNullOrWhiteSpace(query.Search)
            ? string.Empty
            : $" AND {definition.SearchColumn} LIKE @Search ESCAPE '~'";
        var fromAndWhere = $"""
            FROM dbo.{definition.Table} w
            INNER JOIN dbo.TeamTechnology technology ON technology.TeamTechnologyId = w.TeamTechnologyId
            INNER JOIN dbo.{definition.StatusTable} status ON status.StatusId = w.StatusId
            WHERE w.IsActive = 1{pendingFilter}{statusFilter}{technologyFilter}{searchFilter}
            """;
        var parameters = new
        {
            TechnologyId = query.TechnologyId,
            StatusId = query.StatusId,
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{EscapeLikePattern(query.Search.Trim())}%",
            Offset = (query.Page - 1) * query.PageSize,
            query.PageSize
        };
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var total = await connection.QuerySingleAsync<int>(
            new CommandDefinition($"SELECT COUNT(1) {fromAndWhere};", parameters, cancellationToken: cancellationToken));
        var items = await connection.QueryAsync<WorkItemListRow>(
            new CommandDefinition($"""
                SELECT CONVERT(nvarchar(100), w.{definition.IdColumn}) AS Id,
                    technology.Name AS Technology,
                    {definition.DescriptionColumn} AS Description,
                    w.{definition.DateColumn} AS RegistrationDate,
                    status.Name AS Status
                {fromAndWhere}
                ORDER BY {sortColumn} {direction}, w.{definition.IdColumn} ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                """, parameters, cancellationToken: cancellationToken));
        return new PagedResult<WorkItemListRow>(items.ToArray(), total, query.Page, query.PageSize);
    }

    public Task<int> CreateFindingAsync(FindingInput input, int userId, CancellationToken cancellationToken) =>
        InTransactionAsync(async (connection, transaction) =>
        {
            var id = await ReserveIdAsync(connection, transaction, "Finding", cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT dbo.Finding
                    (FindingId, RegistrationDate, TramiteId, SubstageId, TeamTechnologyId, CategoryId,
                     Description, ResponsiblePersonId, StatusId, CriticalityId, PriorityId,
                     RegistrationPhaseId, AttentionPhaseId, CommitmentDate, SolutionDate, CreatedByUserId)
                VALUES
                    (@Id, @RegistrationDate, @TramiteId, @SubstageId, @TechnologyId, @CategoryId,
                     @Description, @ResponsiblePersonId, @StatusId, @CriticalityId, @PriorityId,
                     @RegistrationPhaseId, @AttentionPhaseId, @CommitmentDate, @SolutionDate, @UserId);
                """, new
            {
                Id = id, input.RegistrationDate, input.TramiteId, input.SubstageId,
                input.TechnologyId, input.CategoryId, input.Description,
                input.ResponsiblePersonId, input.StatusId, input.CriticalityId,
                input.PriorityId, input.RegistrationPhaseId, input.AttentionPhaseId,
                input.CommitmentDate, input.SolutionDate, UserId = userId
            }, transaction, cancellationToken: cancellationToken));
            return id;
        }, cancellationToken);

    public async Task UpdateFindingAsync(int id, FindingInput input, int userId, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE dbo.Finding SET RegistrationDate = @RegistrationDate, TramiteId = @TramiteId,
                SubstageId = @SubstageId, TeamTechnologyId = @TechnologyId, CategoryId = @CategoryId,
                Description = @Description, ResponsiblePersonId = @ResponsiblePersonId, StatusId = @StatusId,
                CriticalityId = @CriticalityId, PriorityId = @PriorityId,
                RegistrationPhaseId = @RegistrationPhaseId, AttentionPhaseId = @AttentionPhaseId,
                CommitmentDate = @CommitmentDate, SolutionDate = @SolutionDate,
                UpdatedAtUtc = SYSUTCDATETIME(), UpdatedByUserId = @UserId
            WHERE FindingId = @Id AND IsActive = 1;
            """, new { input.RegistrationDate, input.TramiteId, input.SubstageId, input.TechnologyId,
            input.CategoryId, input.Description, input.ResponsiblePersonId, input.StatusId,
            input.CriticalityId, input.PriorityId, input.RegistrationPhaseId, input.AttentionPhaseId,
            input.CommitmentDate, input.SolutionDate, Id = id, UserId = userId }, cancellationToken: cancellationToken));
        EnsureUpdated(affected, "Finding", id);
    }

    public async Task<FindingInput?> GetFindingAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<FindingInput>(new CommandDefinition("""
            SELECT RegistrationDate, TramiteId, SubstageId, TeamTechnologyId AS TechnologyId,
                CategoryId, Description, ResponsiblePersonId, StatusId, CriticalityId, PriorityId,
                RegistrationPhaseId, AttentionPhaseId, CommitmentDate, SolutionDate
            FROM dbo.Finding WHERE FindingId = @Id AND IsActive = 1;
            """, new { Id = id }, cancellationToken: cancellationToken));
    }

    public Task SoftDeleteFindingAsync(int id, int userId, CancellationToken cancellationToken) =>
        SoftDeleteAsync("Finding", "FindingId", id, userId, cancellationToken);

    public async Task AddFindingFollowUpAsync(int id, FollowUpInput input, int userId, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition("""
            INSERT dbo.FindingFollowUp
                (FindingId, FollowUpDate, UserId, Observation, OldStatusId, NewStatusId,
                 OldCommitmentDate, NewCommitmentDate, CreatedByUserId)
            SELECT @Id, @Date, @UserId, @Observation, @OldStatusId, @NewStatusId,
                @OldCommitmentDate, @NewCommitmentDate, @UserId
            WHERE EXISTS (SELECT 1 FROM dbo.Finding WHERE FindingId = @Id AND IsActive = 1);
            """, new { Id = id, input.Date, UserId = userId, input.Observation, input.OldStatusId,
            input.NewStatusId, input.OldCommitmentDate, input.NewCommitmentDate }, cancellationToken: cancellationToken));
        EnsureUpdated(affected, "Finding", id);
    }

    public Task<IReadOnlyList<FollowUpRow>> GetFindingFollowUpsAsync(int id, CancellationToken cancellationToken) =>
        GetFollowUpsAsync("FindingFollowUp", "FindingId", "FindingStatus", id, cancellationToken);

    public Task<int> CreateAgreementAsync(AgreementInput input, int userId, CancellationToken cancellationToken) =>
        InTransactionAsync(async (connection, transaction) =>
        {
            var id = await ReserveIdAsync(connection, transaction, "Agreement", cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT dbo.Agreement
                    (AgreementId, RegistrationDate, TramiteId, TeamTechnologyId, Description,
                     ResponsiblePersonId, StatusId, CommitmentDate, SolutionDate, CreatedByUserId)
                VALUES
                    (@Id, @RegistrationDate, @TramiteId, @TechnologyId, @Description,
                     @ResponsiblePersonId, @StatusId, @CommitmentDate, @SolutionDate, @UserId);
                """, new { Id = id, input.RegistrationDate, input.TramiteId, input.TechnologyId,
                input.Description, input.ResponsiblePersonId, input.StatusId, input.CommitmentDate,
                input.SolutionDate, UserId = userId }, transaction, cancellationToken: cancellationToken));
            return id;
        }, cancellationToken);

    public async Task UpdateAgreementAsync(int id, AgreementInput input, int userId, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE dbo.Agreement SET RegistrationDate = @RegistrationDate, TramiteId = @TramiteId,
                TeamTechnologyId = @TechnologyId, Description = @Description,
                ResponsiblePersonId = @ResponsiblePersonId, StatusId = @StatusId,
                CommitmentDate = @CommitmentDate, SolutionDate = @SolutionDate,
                UpdatedAtUtc = SYSUTCDATETIME(), UpdatedByUserId = @UserId
            WHERE AgreementId = @Id AND IsActive = 1;
            """, new { input.RegistrationDate, input.TramiteId, input.TechnologyId, input.Description,
            input.ResponsiblePersonId, input.StatusId, input.CommitmentDate, input.SolutionDate,
            Id = id, UserId = userId }, cancellationToken: cancellationToken));
        EnsureUpdated(affected, "Agreement", id);
    }

    public async Task<AgreementInput?> GetAgreementAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AgreementInput>(new CommandDefinition("""
            SELECT RegistrationDate, TramiteId, TeamTechnologyId AS TechnologyId, Description,
                ResponsiblePersonId, StatusId, CommitmentDate, SolutionDate
            FROM dbo.Agreement WHERE AgreementId = @Id AND IsActive = 1;
            """, new { Id = id }, cancellationToken: cancellationToken));
    }

    public Task SoftDeleteAgreementAsync(int id, int userId, CancellationToken cancellationToken) =>
        SoftDeleteAsync("Agreement", "AgreementId", id, userId, cancellationToken);

    public async Task AddAgreementFollowUpAsync(int id, FollowUpInput input, int userId, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition("""
            INSERT dbo.AgreementFollowUp
                (AgreementId, FollowUpDate, UserId, Observation, OldStatusId, NewStatusId,
                 OldCommitmentDate, NewCommitmentDate, CreatedByUserId)
            SELECT @Id, @Date, @UserId, @Observation, @OldStatusId, @NewStatusId,
                @OldCommitmentDate, @NewCommitmentDate, @UserId
            WHERE EXISTS (SELECT 1 FROM dbo.Agreement WHERE AgreementId = @Id AND IsActive = 1);
            """, new { Id = id, input.Date, UserId = userId, input.Observation, input.OldStatusId,
            input.NewStatusId, input.OldCommitmentDate, input.NewCommitmentDate }, cancellationToken: cancellationToken));
        EnsureUpdated(affected, "Agreement", id);
    }

    public Task<IReadOnlyList<FollowUpRow>> GetAgreementFollowUpsAsync(int id, CancellationToken cancellationToken) =>
        GetFollowUpsAsync("AgreementFollowUp", "AgreementId", "AgreementStatus", id, cancellationToken);

    public Task<string> CreateDecisionAsync(DecisionInput input, int userId, CancellationToken cancellationToken) =>
        InTransactionAsync(async (connection, transaction) =>
        {
            var suffix = await ReserveIdAsync(connection, transaction, "ArchitectureDecisionSuffix", cancellationToken);
            var id = $"TRAMITE{input.TramiteId}-SUB{input.SubstageId}-{suffix:000}";
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT dbo.ArchitectureDecision
                    (DecisionId, TramiteId, SubstageId, TeamTechnologyId, ResponsibleTeamTechnologyId,
                     ProblemDescription, SolutionApproach, DecisionText, RegistrationDate, StatusId, CreatedByUserId)
                VALUES
                    (@Id, @TramiteId, @SubstageId, @TechnologyId, @ResponsibleTeamTechnologyId,
                     @ProblemDescription, @SolutionApproach, @DecisionText, @RegistrationDate, @StatusId, @UserId);
                """, new { Id = id, input.TramiteId, input.SubstageId, input.TechnologyId,
                input.ResponsibleTeamTechnologyId, input.ProblemDescription, input.SolutionApproach,
                input.DecisionText, input.RegistrationDate, input.StatusId, UserId = userId },
                transaction, cancellationToken: cancellationToken));
            return id;
        }, cancellationToken);

    public async Task UpdateDecisionAsync(string id, DecisionInput input, int userId, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE dbo.ArchitectureDecision SET TramiteId = @TramiteId, SubstageId = @SubstageId,
                TeamTechnologyId = @TechnologyId, ResponsibleTeamTechnologyId = @ResponsibleTeamTechnologyId,
                ProblemDescription = @ProblemDescription, SolutionApproach = @SolutionApproach,
                DecisionText = @DecisionText, RegistrationDate = @RegistrationDate, StatusId = @StatusId,
                UpdatedAtUtc = SYSUTCDATETIME(), UpdatedByUserId = @UserId
            WHERE DecisionId = @Id AND IsActive = 1;
            """, new { input.TramiteId, input.SubstageId, input.TechnologyId, input.ResponsibleTeamTechnologyId,
            input.ProblemDescription, input.SolutionApproach, input.DecisionText, input.RegistrationDate,
            input.StatusId, Id = id, UserId = userId }, cancellationToken: cancellationToken));
        EnsureUpdated(affected, "ArchitectureDecision", id);
    }

    public async Task<DecisionInput?> GetDecisionAsync(string id, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<DecisionInput>(new CommandDefinition("""
            SELECT TramiteId, SubstageId, TeamTechnologyId AS TechnologyId,
                ResponsibleTeamTechnologyId, ProblemDescription, SolutionApproach, DecisionText,
                RegistrationDate, StatusId
            FROM dbo.ArchitectureDecision WHERE DecisionId = @Id AND IsActive = 1;
            """, new { Id = id }, cancellationToken: cancellationToken));
    }

    public Task SoftDeleteDecisionAsync(string id, int userId, CancellationToken cancellationToken) =>
        SoftDeleteAsync("ArchitectureDecision", "DecisionId", id, userId, cancellationToken);

    public Task<int> CreateMeetingAsync(MeetingInput input, int userId, CancellationToken cancellationToken) =>
        InTransactionAsync(async (connection, transaction) =>
        {
            var id = await ReserveIdAsync(connection, transaction, "Meeting", cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT dbo.Meeting
                    (MeetingId, TramiteId, TeamTechnologyId, MeetingDate, MeetingTime, StatusId,
                     AgreementsSummary, CommitmentsSummary, Subject, Comments, CreatedByUserId)
                VALUES
                    (@Id, @TramiteId, @TechnologyId, @Date, @Time, @StatusId,
                     @AgreementsSummary, @CommitmentsSummary, @Subject, @Comments, @UserId);
                """, new { Id = id, input.TramiteId, input.TechnologyId, input.Date, input.Time,
                input.StatusId, input.AgreementsSummary, input.CommitmentsSummary, input.Subject,
                input.Comments, UserId = userId }, transaction, cancellationToken: cancellationToken));
            await InsertParticipantsAsync(connection, transaction, id, input.Participants, userId, cancellationToken);
            return id;
        }, cancellationToken);

    public async Task UpdateMeetingAsync(int id, MeetingInput input, int userId, CancellationToken cancellationToken)
    {
        await InTransactionAsync(async (connection, transaction) =>
        {
            var affected = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE dbo.Meeting SET TramiteId = @TramiteId, TeamTechnologyId = @TechnologyId,
                    MeetingDate = @Date, MeetingTime = @Time, StatusId = @StatusId,
                    AgreementsSummary = @AgreementsSummary, CommitmentsSummary = @CommitmentsSummary,
                    Subject = @Subject, Comments = @Comments, UpdatedAtUtc = SYSUTCDATETIME(),
                    UpdatedByUserId = @UserId
                WHERE MeetingId = @Id AND IsActive = 1;
                """, new { input.TramiteId, input.TechnologyId, input.Date, input.Time, input.StatusId,
                input.AgreementsSummary, input.CommitmentsSummary, input.Subject, input.Comments,
                Id = id, UserId = userId }, transaction, cancellationToken: cancellationToken));
            EnsureUpdated(affected, "Meeting", id);
            await connection.ExecuteAsync(new CommandDefinition("DELETE dbo.MeetingParticipant WHERE MeetingId = @Id;",
                new { Id = id }, transaction, cancellationToken: cancellationToken));
            await InsertParticipantsAsync(connection, transaction, id, input.Participants, userId, cancellationToken);
            return true;
        }, cancellationToken);
    }

    public async Task<MeetingInput?> GetMeetingAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var meeting = await connection.QuerySingleOrDefaultAsync<MeetingHeader>(new CommandDefinition("""
            SELECT TramiteId, TeamTechnologyId AS TechnologyId, MeetingDate AS Date, MeetingTime AS Time,
                StatusId, AgreementsSummary, CommitmentsSummary, Subject, Comments
            FROM dbo.Meeting WHERE MeetingId = @Id AND IsActive = 1;
            """, new { Id = id }, cancellationToken: cancellationToken));
        if (meeting is null) return null;
        var participants = (await connection.QueryAsync<MeetingParticipantInput>(new CommandDefinition("""
            SELECT PersonId, CONVERT(bit, CASE WHEN Attendance = 'Attended' THEN 1 ELSE 0 END) AS Attended
            FROM dbo.MeetingParticipant WHERE MeetingId = @Id ORDER BY PersonId;
            """, new { Id = id }, cancellationToken: cancellationToken))).ToArray();
        return new MeetingInput(meeting.TramiteId, meeting.TechnologyId, meeting.Date, meeting.Time,
            meeting.StatusId, participants, meeting.AgreementsSummary, meeting.CommitmentsSummary,
            meeting.Subject, meeting.Comments);
    }

    public async Task<MeetingMinuteInput?> GetMeetingMinuteAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<MeetingMinuteInput>(new CommandDefinition("""
            SELECT OriginalFileName, StorageFileName, RelativePath
            FROM dbo.MeetingMinute
            WHERE MeetingId = @Id
              AND EXISTS (SELECT 1 FROM dbo.Meeting WHERE MeetingId = @Id AND IsActive = 1);
            """, new { Id = id }, cancellationToken: cancellationToken));
    }

    public Task SoftDeleteMeetingAsync(int id, int userId, CancellationToken cancellationToken) =>
        SoftDeleteAsync("Meeting", "MeetingId", id, userId, cancellationToken);

    public async Task ReplaceMeetingMinuteAsync(int id, MeetingMinuteInput input, int userId, CancellationToken cancellationToken)
    {
        await InTransactionAsync(async (connection, transaction) =>
        {
            var oldPath = await connection.QuerySingleOrDefaultAsync<string>(new CommandDefinition(
                "SELECT RelativePath FROM dbo.MeetingMinute WITH (UPDLOCK, HOLDLOCK) WHERE MeetingId = @Id;",
                new { Id = id }, transaction, cancellationToken: cancellationToken));
            if (oldPath is null)
            {
                var meetingExists = await connection.QuerySingleAsync<bool>(new CommandDefinition(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Meeting WHERE MeetingId = @Id AND IsActive = 1) THEN 1 ELSE 0 END;",
                    new { Id = id }, transaction, cancellationToken: cancellationToken));
                if (!meetingExists) throw new KeyNotFoundException($"Meeting {id} was not found.");
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT dbo.MeetingMinute (MeetingId, OriginalFileName, StorageFileName, RelativePath, CreatedByUserId)
                    VALUES (@Id, @OriginalFileName, @StorageFileName, @RelativePath, @UserId);
                    """, new { Id = id, input.OriginalFileName, input.StorageFileName, input.RelativePath, UserId = userId },
                    transaction, cancellationToken: cancellationToken));
            }
            else
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    UPDATE dbo.MeetingMinute SET OriginalFileName = @OriginalFileName,
                        StorageFileName = @StorageFileName, RelativePath = @RelativePath,
                        UpdatedAtUtc = SYSUTCDATETIME(), UpdatedByUserId = @UserId
                    WHERE MeetingId = @Id;
                    """, new { Id = id, input.OriginalFileName, input.StorageFileName, input.RelativePath, UserId = userId },
                    transaction, cancellationToken: cancellationToken));
                if (!string.Equals(oldPath, input.RelativePath, StringComparison.Ordinal))
                {
                    await connection.ExecuteAsync(new CommandDefinition("""
                        INSERT dbo.FileCleanupQueue (RelativePath) VALUES (@RelativePath);
                        """, new { RelativePath = oldPath }, transaction, cancellationToken: cancellationToken));
                }
            }
            return true;
        }, cancellationToken);
    }

    private async Task SoftDeleteAsync(string table, string idColumn, object id, int userId, CancellationToken cancellationToken)
    {
        var allowed = table switch
        {
            "Finding" => ("Finding", "FindingId"),
            "Agreement" => ("Agreement", "AgreementId"),
            "ArchitectureDecision" => ("ArchitectureDecision", "DecisionId"),
            "Meeting" => ("Meeting", "MeetingId"),
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };
        if (allowed.Item1 != table || allowed.Item2 != idColumn)
            throw new ArgumentException("Unsupported soft-delete target.");
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition($"""
            UPDATE dbo.{table} SET IsActive = 0, DeletedAtUtc = SYSUTCDATETIME(),
                DeletedByUserId = @UserId, UpdatedAtUtc = SYSUTCDATETIME(), UpdatedByUserId = @UserId
            WHERE {idColumn} = @Id AND IsActive = 1;
            """, new { Id = id, UserId = userId }, cancellationToken: cancellationToken));
        EnsureUpdated(affected, table, id);
    }

    private async Task<IReadOnlyList<FollowUpRow>> GetFollowUpsAsync(
        string table,
        string parentColumn,
        string statusTable,
        int id,
        CancellationToken cancellationToken)
    {
        var allowed = table switch
        {
            "FindingFollowUp" => ("FindingFollowUp", "FindingId", "FindingStatus"),
            "AgreementFollowUp" => ("AgreementFollowUp", "AgreementId", "AgreementStatus"),
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };
        if (allowed.Item1 != table || allowed.Item2 != parentColumn || allowed.Item3 != statusTable)
            throw new ArgumentException("Unsupported follow-up target.");
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<FollowUpRow>(new CommandDefinition($"""
            SELECT f.FollowUpDate AS Date, u.DisplayName AS UserName, f.Observation,
                oldStatus.Name AS OldStatus, newStatus.Name AS NewStatus,
                f.OldCommitmentDate, f.NewCommitmentDate
            FROM dbo.{table} f
            INNER JOIN dbo.ConfiguredUser u ON u.ConfiguredUserId = f.UserId
            LEFT JOIN dbo.{statusTable} oldStatus ON oldStatus.StatusId = f.OldStatusId
            LEFT JOIN dbo.{statusTable} newStatus ON newStatus.StatusId = f.NewStatusId
            WHERE f.{parentColumn} = @Id ORDER BY f.FollowUpDate, f.{table}Id;
            """, new { Id = id }, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    private static async Task InsertParticipantsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int meetingId,
        IReadOnlyCollection<MeetingParticipantInput> participants,
        int userId,
        CancellationToken cancellationToken)
    {
        foreach (var participant in participants)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT dbo.MeetingParticipant (MeetingId, PersonId, Attendance, CreatedByUserId)
                VALUES (@MeetingId, @PersonId, @Attendance, @UserId);
                """, new { MeetingId = meetingId, participant.PersonId, Attendance = participant.Attended ? "Attended" : "Absent", UserId = userId },
                transaction, cancellationToken: cancellationToken));
        }
    }

    private async Task<int> ReserveIdAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string counterKey,
        CancellationToken cancellationToken) =>
        await connection.QuerySingleAsync<int>(new CommandDefinition("""
            UPDATE dbo.ConsecutiveIdentifierCounter WITH (UPDLOCK, ROWLOCK)
            SET LastValue = LastValue + 1
            OUTPUT INSERTED.LastValue
            WHERE CounterKey = @CounterKey;
            """, new { CounterKey = counterKey }, transaction, cancellationToken: cancellationToken));

    private async Task<T> InTransactionAsync<T>(
        Func<SqlConnection, SqlTransaction, Task<T>> action,
        CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(connection, transaction);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Database transaction failed and will be rolled back.");
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackError)
            {
                logger.LogError(rollbackError, "Database transaction rollback failed.");
                throw new AggregateException("The database operation and its rollback both failed.",
                    exception, rollbackError);
            }
            throw;
        }
    }

    private static void EnsureUpdated(int affected, string entity, object id)
    {
        if (affected == 0) throw new KeyNotFoundException($"{entity} {id} was not found or is inactive.");
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace("~", "~~", StringComparison.Ordinal)
            .Replace("%", "~%", StringComparison.Ordinal)
            .Replace("_", "~_", StringComparison.Ordinal)
            .Replace("[", "~[", StringComparison.Ordinal);

    private static ListDefinition GetListDefinition(WorkItemKind kind) => kind switch
    {
        WorkItemKind.Finding => new("Finding", "FindingStatus", "FindingId", "RegistrationDate",
            "Description", "w.Description", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = "w.FindingId", ["Technology"] = "technology.Name",
                ["Status"] = "status.Name", ["RegistrationDate"] = "w.RegistrationDate"
            }),
        WorkItemKind.Agreement => new("Agreement", "AgreementStatus", "AgreementId", "RegistrationDate",
            "Description", "w.Description", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = "w.AgreementId", ["Technology"] = "technology.Name",
                ["Status"] = "status.Name", ["RegistrationDate"] = "w.RegistrationDate"
            }),
        WorkItemKind.Decision => new("ArchitectureDecision", "DecisionStatus", "DecisionId", "RegistrationDate",
            "ProblemDescription", "w.ProblemDescription", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["RegistrationDate"] = "w.RegistrationDate"
            }),
        WorkItemKind.Meeting => new("Meeting", "MeetingStatus", "MeetingId", "MeetingDate",
            "Subject", "w.Subject", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = "w.MeetingId", ["Technology"] = "technology.Name",
                ["Subject"] = "w.Subject", ["Date"] = "w.MeetingDate"
            }),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    private sealed record ListDefinition(
        string Table,
        string StatusTable,
        string IdColumn,
        string DateColumn,
        string DescriptionColumn,
        string SearchColumn,
        IReadOnlyDictionary<string, string> SortColumns);

    private sealed record MeetingHeader(
        int TramiteId,
        int TechnologyId,
        DateOnly Date,
        TimeOnly Time,
        int StatusId,
        string AgreementsSummary,
        string CommitmentsSummary,
        string Subject,
        string? Comments);
}
