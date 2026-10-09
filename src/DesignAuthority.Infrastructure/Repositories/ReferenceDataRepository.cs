using Dapper;
using DesignAuthority.Application.Abstractions;
using DesignAuthority.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace DesignAuthority.Infrastructure.Repositories;

public sealed class ReferenceDataRepository(SqlConnectionFactory connections) : IReferenceDataRepository
{
    public async Task<WorkItemFormOptions> GetWorkItemFormOptionsAsync(
        WorkItemKind kind,
        int? tramiteId,
        CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        var tramites = await OptionsAsync(connection,
            "SELECT TramiteId AS Id, Name FROM dbo.Tramite WHERE IsActive = 1 ORDER BY Name;", cancellationToken);
        var defaultId = await connection.QuerySingleOrDefaultAsync<int?>(
            new CommandDefinition("SELECT TOP (1) TramiteId FROM dbo.Tramite WHERE IsActive = 1 AND IsDefault = 1 ORDER BY TramiteId;",
                cancellationToken: cancellationToken));
        var substageSql = kind switch
        {
            WorkItemKind.Finding => "SELECT SubstageId AS Id, Name, TramiteId AS ParentId FROM dbo.Substage WHERE IsActive = 1 ORDER BY Name;",
            WorkItemKind.Decision => "SELECT SubstageId AS Id, Name, TramiteId AS ParentId FROM dbo.Substage ORDER BY Name;",
            _ => null
        };
        var substages = substageSql is null
            ? Array.Empty<ReferenceOption>()
            : await OptionsAsync(connection, substageSql, cancellationToken);
        var technologies = await OptionsAsync(connection,
            "SELECT TeamTechnologyId AS Id, Name FROM dbo.TeamTechnology WHERE Kind = 'Technology' ORDER BY Name;", cancellationToken);
        var teams = await OptionsAsync(connection,
            "SELECT TeamTechnologyId AS Id, Name FROM dbo.TeamTechnology WHERE Kind = 'Team' ORDER BY Name;", cancellationToken);
        var people = await OptionsAsync(connection,
            "SELECT PersonId AS Id, CONCAT(Name, N' ', PaternalSurname, CASE WHEN MaternalSurname IS NULL THEN N'' ELSE N' ' + MaternalSurname END) AS Name FROM dbo.Person ORDER BY PaternalSurname, Name;", cancellationToken);

        var categories = kind == WorkItemKind.Finding
            ? await OptionsAsync(connection, "SELECT CategoryId AS Id, Name FROM dbo.FindingCategory ORDER BY Name;", cancellationToken)
            : Array.Empty<ReferenceOption>();
        var statuses = await OptionsAsync(connection, StatusQuery(kind), cancellationToken);
        var criticalities = kind == WorkItemKind.Finding
            ? await OptionsAsync(connection, "SELECT CriticalityId AS Id, Name FROM dbo.Criticality ORDER BY CriticalityId;", cancellationToken)
            : Array.Empty<ReferenceOption>();
        var priorities = kind == WorkItemKind.Finding
            ? await OptionsAsync(connection, "SELECT PriorityId AS Id, Name FROM dbo.Priority ORDER BY PriorityId;", cancellationToken)
            : Array.Empty<ReferenceOption>();
        var registrationPhases = kind == WorkItemKind.Finding
            ? await OptionsAsync(connection, "SELECT RegistrationPhaseId AS Id, Name FROM dbo.RegistrationPhase ORDER BY RegistrationPhaseId;", cancellationToken)
            : Array.Empty<ReferenceOption>();
        var attentionPhases = kind == WorkItemKind.Finding
            ? await OptionsAsync(connection, "SELECT AttentionPhaseId AS Id, Name FROM dbo.AttentionPhase ORDER BY AttentionPhaseId;", cancellationToken)
            : Array.Empty<ReferenceOption>();

        return new WorkItemFormOptions(defaultId, tramites, substages, technologies, teams, people,
            categories, statuses, criticalities, priorities, registrationPhases, attentionPhases);
    }

    public Task<bool> IsActiveTramiteAsync(int tramiteId, CancellationToken cancellationToken) =>
        ExistsAsync("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Tramite WHERE TramiteId = @Id AND IsActive = 1) THEN 1 ELSE 0 END;",
            tramiteId, cancellationToken);

    public Task<bool> IsTechnologyAsync(int teamTechnologyId, CancellationToken cancellationToken) =>
        ExistsAsync("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.TeamTechnology WHERE TeamTechnologyId = @Id AND Kind = 'Technology') THEN 1 ELSE 0 END;",
            teamTechnologyId, cancellationToken);

    public Task<bool> IsTeamAsync(int teamTechnologyId, CancellationToken cancellationToken) =>
        ExistsAsync("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.TeamTechnology WHERE TeamTechnologyId = @Id AND Kind = 'Team') THEN 1 ELSE 0 END;",
            teamTechnologyId, cancellationToken);

    public Task<bool> IsPersonAsync(int personId, CancellationToken cancellationToken) =>
        ExistsAsync("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Person WHERE PersonId = @Id) THEN 1 ELSE 0 END;",
            personId, cancellationToken);

    public Task<bool> IsActiveSubstageForTramiteAsync(int substageId, int tramiteId, CancellationToken cancellationToken) =>
        ExistsPairAsync("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Substage WHERE SubstageId = @Id AND TramiteId = @SecondId AND IsActive = 1) THEN 1 ELSE 0 END;",
            substageId, tramiteId, cancellationToken);

    public Task<bool> IsSubstageForTramiteAsync(int substageId, int tramiteId, CancellationToken cancellationToken) =>
        ExistsPairAsync("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Substage WHERE SubstageId = @Id AND TramiteId = @SecondId) THEN 1 ELSE 0 END;",
            substageId, tramiteId, cancellationToken);

    public async Task<bool> IsReferenceValueAsync(ReferenceKind kind, int id, CancellationToken cancellationToken)
    {
        var (table, key) = kind switch
        {
            ReferenceKind.FindingCategory => ("FindingCategory", "CategoryId"),
            ReferenceKind.FindingStatus => ("FindingStatus", "StatusId"),
            ReferenceKind.AgreementStatus => ("AgreementStatus", "StatusId"),
            ReferenceKind.DecisionStatus => ("DecisionStatus", "StatusId"),
            ReferenceKind.MeetingStatus => ("MeetingStatus", "StatusId"),
            ReferenceKind.Criticality => ("Criticality", "CriticalityId"),
            ReferenceKind.Priority => ("Priority", "PriorityId"),
            ReferenceKind.RegistrationPhase => ("RegistrationPhase", "RegistrationPhaseId"),
            ReferenceKind.AttentionPhase => ("AttentionPhase", "AttentionPhaseId"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Reference kind is not a value catalog.")
        };
        return await ExistsAsync($"SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.{table} WHERE {key} = @Id) THEN 1 ELSE 0 END;",
            id, cancellationToken);
    }

    private async Task<bool> ExistsAsync(string sql, int id, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<bool>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    private async Task<bool> ExistsPairAsync(string sql, int id, int secondId, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<bool>(new CommandDefinition(sql, new { Id = id, SecondId = secondId }, cancellationToken: cancellationToken));
    }

    private static async Task<ReferenceOption[]> OptionsAsync(
        SqlConnection connection,
        string sql,
        CancellationToken cancellationToken,
        object? parameters = null) =>
        (await connection.QueryAsync<ReferenceOption>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToArray();

    private static string StatusQuery(WorkItemKind kind) => kind switch
    {
        WorkItemKind.Finding => "SELECT StatusId AS Id, Name FROM dbo.FindingStatus ORDER BY StatusId;",
        WorkItemKind.Agreement => "SELECT StatusId AS Id, Name FROM dbo.AgreementStatus ORDER BY StatusId;",
        WorkItemKind.Decision => "SELECT StatusId AS Id, Name FROM dbo.DecisionStatus ORDER BY StatusId;",
        WorkItemKind.Meeting => "SELECT StatusId AS Id, Name FROM dbo.MeetingStatus ORDER BY StatusId;",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}
