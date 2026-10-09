using System.ComponentModel.DataAnnotations;
using DesignAuthority.Application.Abstractions;

namespace DesignAuthority.Web.ViewModels.Meetings;

public sealed class MeetingViewModel
{
    [Range(1, int.MaxValue)]
    public int TramiteId { get; set; }
    [Range(1, int.MaxValue)]
    public int TechnologyId { get; set; }
    [Required, DataType(DataType.Date)]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Required]
    public TimeOnly Time { get; set; }
    [Range(1, int.MaxValue)]
    public int StatusId { get; set; } = 1;
    [Required]
    public string AgreementsSummary { get; set; } = string.Empty;
    [Required]
    public string CommitmentsSummary { get; set; } = string.Empty;
    [Required]
    public string Subject { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public int[] AttendedPersonIds { get; set; } = [];
    public int[] AbsentPersonIds { get; set; } = [];
    public WorkItemFormOptions? Options { get; set; }

    public MeetingInput ToInput() => new(
        TramiteId,
        TechnologyId,
        Date,
        Time,
        StatusId,
        AttendedPersonIds.Select(id => new MeetingParticipantInput(id, true))
            .Concat(AbsentPersonIds.Select(id => new MeetingParticipantInput(id, false))).ToArray(),
        AgreementsSummary,
        CommitmentsSummary,
        Subject,
        Comments);

    public static MeetingViewModel FromInput(MeetingInput input, WorkItemFormOptions options) => new()
    {
        TramiteId = input.TramiteId,
        TechnologyId = input.TechnologyId,
        Date = input.Date,
        Time = input.Time,
        StatusId = input.StatusId,
        AgreementsSummary = input.AgreementsSummary,
        CommitmentsSummary = input.CommitmentsSummary,
        Subject = input.Subject,
        Comments = input.Comments,
        AttendedPersonIds = input.Participants.Where(person => person.Attended).Select(person => person.PersonId).ToArray(),
        AbsentPersonIds = input.Participants.Where(person => !person.Attended).Select(person => person.PersonId).ToArray(),
        Options = options
    };
}
