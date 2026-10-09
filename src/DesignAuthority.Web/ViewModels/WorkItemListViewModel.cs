using DesignAuthority.Application.Abstractions;

namespace DesignAuthority.Web.ViewModels;

public sealed record WorkItemListViewModel(
    WorkItemKind Kind,
    PagedResult<WorkItemListRow> Page,
    string? Search,
    int? TechnologyId,
    int? StatusId,
    string SortBy,
    bool Descending,
    IReadOnlyList<ReferenceOption> Technologies,
    IReadOnlyList<ReferenceOption> Statuses);

public sealed record WorkItemDetailsViewModel(
    string Title,
    string Id,
    string Controller,
    IReadOnlyList<KeyValuePair<string, string?>> Fields,
    string? FollowUpsAction = null,
    string? CurrentMinuteFileName = null)
{
    public static WorkItemDetailsViewModel Create(
        string title,
        string id,
        string controller,
        IEnumerable<(string Name, object? Value)> fields,
        string? followUpsAction = null,
        string? currentMinuteFileName = null) =>
        new(title, id, controller,
            fields.Select(field => new KeyValuePair<string, string?>(field.Name, Format(field.Value))).ToArray(),
            followUpsAction, currentMinuteFileName);

    private static string? Format(object? value) => value switch
    {
        null => null,
        DateOnly date => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        TimeOnly time => time.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
    };
}

public sealed class FollowUpViewModel
{
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string Observation { get; set; } = string.Empty;
    public int? OldStatusId { get; set; }
    public int? NewStatusId { get; set; }
    public DateOnly? OldCommitmentDate { get; set; }
    public DateOnly? NewCommitmentDate { get; set; }
    public IReadOnlyList<ReferenceOption> Statuses { get; set; } = [];
}
