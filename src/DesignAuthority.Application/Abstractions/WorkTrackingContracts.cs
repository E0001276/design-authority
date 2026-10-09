namespace DesignAuthority.Application.Abstractions;

public enum WorkItemKind
{
    Finding,
    Agreement,
    Decision,
    Meeting
}

public enum ReferenceKind
{
    Tramite,
    Substage,
    Technology,
    Team,
    Person,
    FindingCategory,
    FindingStatus,
    AgreementStatus,
    DecisionStatus,
    MeetingStatus,
    Criticality,
    Priority,
    RegistrationPhase,
    AttentionPhase
}

public sealed record ReferenceOption(int Id, string Name, int? ParentId = null);

public sealed record WorkItemFormOptions(
    int? DefaultTramiteId,
    IReadOnlyList<ReferenceOption> Tramites,
    IReadOnlyList<ReferenceOption> Substages,
    IReadOnlyList<ReferenceOption> Technologies,
    IReadOnlyList<ReferenceOption> Teams,
    IReadOnlyList<ReferenceOption> People,
    IReadOnlyList<ReferenceOption> Categories,
    IReadOnlyList<ReferenceOption> Statuses,
    IReadOnlyList<ReferenceOption> Criticalities,
    IReadOnlyList<ReferenceOption> Priorities,
    IReadOnlyList<ReferenceOption> RegistrationPhases,
    IReadOnlyList<ReferenceOption> AttentionPhases);

public sealed record WorkItemQuery(
    string? Search,
    int? TechnologyId,
    int? StatusId,
    string SortBy,
    bool Descending,
    int Page,
    int PageSize);

public sealed record WorkItemListRow(
    string Id,
    string Technology,
    string Description,
    DateOnly RegistrationDate,
    string Status);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record FindingInput(
    DateOnly RegistrationDate,
    int TramiteId,
    int SubstageId,
    int TechnologyId,
    int CategoryId,
    string Description,
    int ResponsiblePersonId,
    int StatusId,
    int CriticalityId,
    int PriorityId,
    int RegistrationPhaseId,
    int AttentionPhaseId,
    DateOnly CommitmentDate,
    DateOnly? SolutionDate);

public sealed record AgreementInput(
    DateOnly RegistrationDate,
    int TramiteId,
    int TechnologyId,
    string Description,
    int ResponsiblePersonId,
    int StatusId,
    DateOnly CommitmentDate,
    DateOnly? SolutionDate);

public sealed record DecisionInput(
    int TramiteId,
    int SubstageId,
    int TechnologyId,
    int ResponsibleTeamTechnologyId,
    string ProblemDescription,
    string SolutionApproach,
    string DecisionText,
    DateOnly RegistrationDate,
    int StatusId);

public sealed record MeetingParticipantInput(int PersonId, bool Attended);

public sealed record MeetingInput(
    int TramiteId,
    int TechnologyId,
    DateOnly Date,
    TimeOnly Time,
    int StatusId,
    IReadOnlyCollection<MeetingParticipantInput> Participants,
    string AgreementsSummary,
    string CommitmentsSummary,
    string Subject,
    string? Comments);

public sealed record FollowUpInput(
    DateOnly Date,
    string Observation,
    int? OldStatusId,
    int? NewStatusId,
    DateOnly? OldCommitmentDate,
    DateOnly? NewCommitmentDate);

public sealed record FollowUpRow(
    DateOnly Date,
    string UserName,
    string Observation,
    string? OldStatus,
    string? NewStatus,
    DateOnly? OldCommitmentDate,
    DateOnly? NewCommitmentDate);

public sealed record MeetingMinuteInput(
    string OriginalFileName,
    string StorageFileName,
    string RelativePath);

public interface IReferenceDataRepository
{
    Task<WorkItemFormOptions> GetWorkItemFormOptionsAsync(WorkItemKind kind, int? tramiteId, CancellationToken cancellationToken);
    Task<bool> IsActiveTramiteAsync(int tramiteId, CancellationToken cancellationToken);
    Task<bool> IsTechnologyAsync(int teamTechnologyId, CancellationToken cancellationToken);
    Task<bool> IsTeamAsync(int teamTechnologyId, CancellationToken cancellationToken);
    Task<bool> IsPersonAsync(int personId, CancellationToken cancellationToken);
    Task<bool> IsActiveSubstageForTramiteAsync(int substageId, int tramiteId, CancellationToken cancellationToken);
    Task<bool> IsSubstageForTramiteAsync(int substageId, int tramiteId, CancellationToken cancellationToken);
    Task<bool> IsReferenceValueAsync(ReferenceKind kind, int id, CancellationToken cancellationToken);
}

public interface IWorkTrackingRepository
{
    Task<PagedResult<WorkItemListRow>> ListAsync(WorkItemKind kind, WorkItemQuery query, CancellationToken cancellationToken);
    Task<int> CreateFindingAsync(FindingInput input, int userId, CancellationToken cancellationToken);
    Task UpdateFindingAsync(int id, FindingInput input, int userId, CancellationToken cancellationToken);
    Task<FindingInput?> GetFindingAsync(int id, CancellationToken cancellationToken);
    Task SoftDeleteFindingAsync(int id, int userId, CancellationToken cancellationToken);
    Task AddFindingFollowUpAsync(int id, FollowUpInput input, int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FollowUpRow>> GetFindingFollowUpsAsync(int id, CancellationToken cancellationToken);
    Task<int> CreateAgreementAsync(AgreementInput input, int userId, CancellationToken cancellationToken);
    Task UpdateAgreementAsync(int id, AgreementInput input, int userId, CancellationToken cancellationToken);
    Task<AgreementInput?> GetAgreementAsync(int id, CancellationToken cancellationToken);
    Task SoftDeleteAgreementAsync(int id, int userId, CancellationToken cancellationToken);
    Task AddAgreementFollowUpAsync(int id, FollowUpInput input, int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FollowUpRow>> GetAgreementFollowUpsAsync(int id, CancellationToken cancellationToken);
    Task<string> CreateDecisionAsync(DecisionInput input, int userId, CancellationToken cancellationToken);
    Task UpdateDecisionAsync(string id, DecisionInput input, int userId, CancellationToken cancellationToken);
    Task<DecisionInput?> GetDecisionAsync(string id, CancellationToken cancellationToken);
    Task SoftDeleteDecisionAsync(string id, int userId, CancellationToken cancellationToken);
    Task<int> CreateMeetingAsync(MeetingInput input, int userId, CancellationToken cancellationToken);
    Task UpdateMeetingAsync(int id, MeetingInput input, int userId, CancellationToken cancellationToken);
    Task<MeetingInput?> GetMeetingAsync(int id, CancellationToken cancellationToken);
    Task<MeetingMinuteInput?> GetMeetingMinuteAsync(int id, CancellationToken cancellationToken);
    Task SoftDeleteMeetingAsync(int id, int userId, CancellationToken cancellationToken);
    Task ReplaceMeetingMinuteAsync(int id, MeetingMinuteInput input, int userId, CancellationToken cancellationToken);
}

public sealed record StoredFile(
    string OriginalFileName,
    string StorageFileName,
    string RelativePath,
    long Length);

public interface IFileStorage
{
    Task<StoredFile> StageAsync(string originalFileName, Stream content, long length, CancellationToken cancellationToken);
    Task PromoteAsync(StoredFile file, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken);
}
