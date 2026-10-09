using System.Net;
using System.Text.RegularExpressions;
using DesignAuthority.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DesignAuthority.Web.IntegrationTests;

public sealed class WorkItemMvcFlowTests : IClassFixture<WorkItemApplicationFactory>
{
    private readonly HttpClient client;

    public WorkItemMvcFlowTests(WorkItemApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Finding_create_page_preselects_default_tramite_and_renders_required_fields()
    {
        var response = await client.GetAsync("/Findings/Create");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertDefaultTramiteSelected(html);
        Assert.Contains("name=\"TechnologyId\"", html);
        Assert.Contains("name=\"SubstageId\"", html);
    }

    [Fact]
    public async Task Meeting_create_page_uses_default_tramite_without_a_substage_field()
    {
        var response = await client.GetAsync("/Meetings/Create");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertDefaultTramiteSelected(html);
        Assert.Contains("name=\"AttendedPersonIds\"", html);
        Assert.DoesNotContain("name=\"SubstageId\"", html);
    }

    [Fact]
    public async Task Finding_list_renders_mandatory_columns_and_consult_action()
    {
        var response = await client.GetAsync("/Findings");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<th>Id</th>", html);
        Assert.Contains("<th>Tecnología</th>", html);
        Assert.Contains("<th>Descripción</th>", html);
        Assert.Contains("Consultar", html);
    }

    private static void AssertDefaultTramiteSelected(string html)
    {
        Assert.Matches(
            new Regex("<option\\b(?=[^>]*\\bvalue=\"1\")(?=[^>]*\\bselected)[^>]*>Trámite predeterminado</option>"),
            html);
    }
}

public sealed class WorkItemApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IReferenceDataRepository>();
            services.RemoveAll<IWorkTrackingRepository>();
            services.RemoveAll<IConfiguredUserContext>();
            services.AddSingleton<IReferenceDataRepository, FakeReferenceDataRepository>();
            services.AddSingleton<IWorkTrackingRepository, FakeWorkTrackingRepository>();
            services.AddSingleton<IConfiguredUserContext>(new FakeConfiguredUserContext());
        });
    }
}

internal sealed class FakeConfiguredUserContext : IConfiguredUserContext
{
    public int UserId => 1;
}

internal sealed class FakeReferenceDataRepository : IReferenceDataRepository
{
    private static readonly ReferenceOption[] Tramites = [new(1, "Trámite predeterminado")];
    private static readonly ReferenceOption[] Substages = [new(2, "Subetapa", 1)];
    private static readonly ReferenceOption[] Technologies = [new(3, "Appian")];
    private static readonly ReferenceOption[] Teams = [new(5, "Arquitectura")];
    private static readonly ReferenceOption[] People = [new(7, "Persona de prueba")];
    private static readonly ReferenceOption[] Categories = [new(1, "Documental")];
    private static readonly ReferenceOption[] FindingStatuses = [new(1, "Abierto"), new(2, "En progreso")];
    private static readonly ReferenceOption[] MeetingStatuses = [new(1, "Realizada")];
    private static readonly ReferenceOption[] MinorLists = [new(1, "Opción")];

    public Task<WorkItemFormOptions> GetWorkItemFormOptionsAsync(
        WorkItemKind kind, int? tramiteId, CancellationToken cancellationToken) =>
        Task.FromResult(new WorkItemFormOptions(
            1, Tramites, kind is WorkItemKind.Finding or WorkItemKind.Decision ? Substages : [],
            Technologies, Teams, People, kind == WorkItemKind.Finding ? Categories : [],
            kind == WorkItemKind.Meeting ? MeetingStatuses : FindingStatuses,
            MinorLists, MinorLists, MinorLists, MinorLists));

    public Task<bool> IsActiveTramiteAsync(int tramiteId, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task<bool> IsTechnologyAsync(int teamTechnologyId, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task<bool> IsTeamAsync(int teamTechnologyId, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task<bool> IsPersonAsync(int personId, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task<bool> IsActiveSubstageForTramiteAsync(int substageId, int tramiteId, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task<bool> IsSubstageForTramiteAsync(int substageId, int tramiteId, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task<bool> IsReferenceValueAsync(ReferenceKind kind, int id, CancellationToken cancellationToken) => Task.FromResult(true);
}

internal sealed class FakeWorkTrackingRepository : IWorkTrackingRepository
{
    public Task<PagedResult<WorkItemListRow>> ListAsync(WorkItemKind kind, WorkItemQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(new PagedResult<WorkItemListRow>(
            [new("1", "Appian", "Hallazgo de prueba", DateOnly.FromDateTime(DateTime.Today), "Abierto")],
            1, query.Page, query.PageSize));
    public Task<int> CreateFindingAsync(FindingInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task UpdateFindingAsync(int id, FindingInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<FindingInput?> GetFindingAsync(int id, CancellationToken cancellationToken) => Task.FromResult<FindingInput?>(null);
    public Task SoftDeleteFindingAsync(int id, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task AddFindingFollowUpAsync(int id, FollowUpInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<IReadOnlyList<FollowUpRow>> GetFindingFollowUpsAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<int> CreateAgreementAsync(AgreementInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task UpdateAgreementAsync(int id, AgreementInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<AgreementInput?> GetAgreementAsync(int id, CancellationToken cancellationToken) => Task.FromResult<AgreementInput?>(null);
    public Task SoftDeleteAgreementAsync(int id, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task AddAgreementFollowUpAsync(int id, FollowUpInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<IReadOnlyList<FollowUpRow>> GetAgreementFollowUpsAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<string> CreateDecisionAsync(DecisionInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task UpdateDecisionAsync(string id, DecisionInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<DecisionInput?> GetDecisionAsync(string id, CancellationToken cancellationToken) => Task.FromResult<DecisionInput?>(null);
    public Task SoftDeleteDecisionAsync(string id, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<int> CreateMeetingAsync(MeetingInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task UpdateMeetingAsync(int id, MeetingInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<MeetingInput?> GetMeetingAsync(int id, CancellationToken cancellationToken) => Task.FromResult<MeetingInput?>(null);
    public Task<MeetingMinuteInput?> GetMeetingMinuteAsync(int id, CancellationToken cancellationToken) => Task.FromResult<MeetingMinuteInput?>(null);
    public Task SoftDeleteMeetingAsync(int id, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task ReplaceMeetingMinuteAsync(int id, MeetingMinuteInput input, int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
}
