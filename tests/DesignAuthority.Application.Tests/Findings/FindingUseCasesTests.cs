using DesignAuthority.Application.Abstractions;
using DesignAuthority.Application.Features;

namespace DesignAuthority.Application.Tests.Findings;

public sealed class FindingUseCasesTests
{
    [Fact]
    public async Task Create_finding_rejects_substage_from_another_tramite_before_writing()
    {
        var repository = new FakeWorkTrackingRepository();
        var references = new FakeReferenceDataRepository { SubstageBelongsToTramite = false };
        var service = CreateService(repository, references);

        var result = await service.CreateFindingAsync(ValidFinding(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(FindingInput.SubstageId), result.Errors.Keys);
        Assert.Equal(0, repository.FindingWrites);
    }

    [Fact]
    public async Task Update_finding_revalidates_category_and_other_reference_values()
    {
        var repository = new FakeWorkTrackingRepository();
        var references = new FakeReferenceDataRepository { InvalidReference = ReferenceKind.FindingCategory };
        var service = CreateService(repository, references);

        var result = await service.UpdateFindingAsync(1, ValidFinding(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(FindingInput.CategoryId), result.Errors.Keys);
        Assert.Equal(0, repository.FindingWrites);
    }

    [Fact]
    public async Task Follow_up_rejects_unpaired_status_changes_before_writing()
    {
        var repository = new FakeWorkTrackingRepository();
        var service = CreateService(repository, new FakeReferenceDataRepository());
        var input = new FollowUpInput(DateOnly.FromDateTime(DateTime.Today), "observación", 1, null, null, null);

        var result = await service.AddFindingFollowUpAsync(1, input, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(FollowUpInput.NewStatusId), result.Errors.Keys);
        Assert.Equal(0, repository.FindingWrites);
    }

    [Fact]
    public async Task List_pagination_is_clamped_before_reaching_the_repository()
    {
        var repository = new FakeWorkTrackingRepository();
        var service = CreateService(repository, new FakeReferenceDataRepository());

        await service.ListAsync(WorkItemKind.Finding,
            new WorkItemQuery(null, null, null, "Id", true, int.MaxValue, int.MaxValue), CancellationToken.None);

        Assert.Equal(100_000, repository.LastQuery!.Page);
        Assert.Equal(100, repository.LastQuery.PageSize);
    }

    [Fact]
    public async Task Meeting_rejects_a_person_selected_as_both_attendee_and_absent()
    {
        var repository = new FakeWorkTrackingRepository();
        var service = CreateService(repository, new FakeReferenceDataRepository());
        var meeting = new MeetingInput(1, 1, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0),
            1, [new MeetingParticipantInput(7, true), new MeetingParticipantInput(7, false)],
            "Acuerdos", "Compromisos", "Asunto", null);

        var result = await service.CreateMeetingAsync(meeting, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(MeetingInput.Participants), result.Errors.Keys);
        Assert.Equal(0, repository.MeetingWrites);
    }

    private static WorkTrackingService CreateService(
        FakeWorkTrackingRepository repository,
        FakeReferenceDataRepository references) =>
        new(repository, references, new FakeConfiguredUserContext());

    private static FindingInput ValidFinding() => new(
        DateOnly.FromDateTime(DateTime.Today), 1, 2, 3, 4, "Descripción", 5, 1,
        1, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), null);

    private sealed class FakeConfiguredUserContext : IConfiguredUserContext
    {
        public int UserId => 1;
    }

    private sealed class FakeReferenceDataRepository : IReferenceDataRepository
    {
        public bool SubstageBelongsToTramite { get; init; } = true;
        public ReferenceKind? InvalidReference { get; init; }

        public Task<WorkItemFormOptions> GetWorkItemFormOptionsAsync(
            WorkItemKind kind, int? tramiteId, CancellationToken cancellationToken) =>
            Task.FromResult(new WorkItemFormOptions(null, [], [], [], [], [], [], [], [], [], [], []));

        public Task<bool> IsActiveTramiteAsync(int tramiteId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsTechnologyAsync(int teamTechnologyId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsTeamAsync(int teamTechnologyId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsPersonAsync(int personId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsActiveSubstageForTramiteAsync(int substageId, int tramiteId, CancellationToken cancellationToken) =>
            Task.FromResult(SubstageBelongsToTramite);

        public Task<bool> IsSubstageForTramiteAsync(int substageId, int tramiteId, CancellationToken cancellationToken) =>
            Task.FromResult(SubstageBelongsToTramite);

        public Task<bool> IsReferenceValueAsync(ReferenceKind kind, int id, CancellationToken cancellationToken) =>
            Task.FromResult(kind != InvalidReference);
    }

    private sealed class FakeWorkTrackingRepository : IWorkTrackingRepository
    {
        public int FindingWrites { get; private set; }
        public int MeetingWrites { get; private set; }
        public WorkItemQuery? LastQuery { get; private set; }

        public Task<PagedResult<WorkItemListRow>> ListAsync(WorkItemKind kind, WorkItemQuery query, CancellationToken cancellationToken)
        {
            LastQuery = query;
            return Task.FromResult(new PagedResult<WorkItemListRow>([], 0, query.Page, query.PageSize));
        }
        public Task<int> CreateFindingAsync(FindingInput input, int userId, CancellationToken cancellationToken)
        {
            FindingWrites++;
            return Task.FromResult(1);
        }
        public Task UpdateFindingAsync(int id, FindingInput input, int userId, CancellationToken cancellationToken)
        {
            FindingWrites++;
            return Task.CompletedTask;
        }
        public Task<FindingInput?> GetFindingAsync(int id, CancellationToken cancellationToken) => Task.FromResult<FindingInput?>(null);
        public Task SoftDeleteFindingAsync(int id, int userId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddFindingFollowUpAsync(int id, FollowUpInput input, int userId, CancellationToken cancellationToken)
        {
            FindingWrites++;
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<FollowUpRow>> GetFindingFollowUpsAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FollowUpRow>>([]);
        public Task<int> CreateAgreementAsync(AgreementInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task UpdateAgreementAsync(int id, AgreementInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AgreementInput?> GetAgreementAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SoftDeleteAgreementAsync(int id, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddAgreementFollowUpAsync(int id, FollowUpInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<FollowUpRow>> GetAgreementFollowUpsAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<string> CreateDecisionAsync(DecisionInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task UpdateDecisionAsync(string id, DecisionInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<DecisionInput?> GetDecisionAsync(string id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SoftDeleteDecisionAsync(string id, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<int> CreateMeetingAsync(MeetingInput input, int userId, CancellationToken cancellationToken)
        {
            MeetingWrites++;
            return Task.FromResult(1);
        }
        public Task UpdateMeetingAsync(int id, MeetingInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<MeetingInput?> GetMeetingAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<MeetingMinuteInput?> GetMeetingMinuteAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SoftDeleteMeetingAsync(int id, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task ReplaceMeetingMinuteAsync(int id, MeetingMinuteInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
