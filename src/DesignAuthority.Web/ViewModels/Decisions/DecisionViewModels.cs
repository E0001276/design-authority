using System.ComponentModel.DataAnnotations;
using DesignAuthority.Application.Abstractions;

namespace DesignAuthority.Web.ViewModels.Decisions;

public sealed class DecisionViewModel
{
    [Range(1, int.MaxValue)]
    public int TramiteId { get; set; }
    [Range(1, int.MaxValue)]
    public int SubstageId { get; set; }
    [Range(1, int.MaxValue)]
    public int TechnologyId { get; set; }
    [Range(1, int.MaxValue)]
    public int ResponsibleTeamTechnologyId { get; set; }
    [Required]
    public string ProblemDescription { get; set; } = string.Empty;
    [Required]
    public string SolutionApproach { get; set; } = string.Empty;
    [Required]
    public string DecisionText { get; set; } = string.Empty;
    [Required, DataType(DataType.Date)]
    public DateOnly RegistrationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Range(1, int.MaxValue)]
    public int StatusId { get; set; } = 1;
    public WorkItemFormOptions? Options { get; set; }

    public DecisionInput ToInput() => new(TramiteId, SubstageId, TechnologyId,
        ResponsibleTeamTechnologyId, ProblemDescription, SolutionApproach, DecisionText,
        RegistrationDate, StatusId);

    public static DecisionViewModel FromInput(DecisionInput input, WorkItemFormOptions options) => new()
    {
        TramiteId = input.TramiteId,
        SubstageId = input.SubstageId,
        TechnologyId = input.TechnologyId,
        ResponsibleTeamTechnologyId = input.ResponsibleTeamTechnologyId,
        ProblemDescription = input.ProblemDescription,
        SolutionApproach = input.SolutionApproach,
        DecisionText = input.DecisionText,
        RegistrationDate = input.RegistrationDate,
        StatusId = input.StatusId,
        Options = options
    };
}
