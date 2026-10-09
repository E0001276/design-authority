using System.ComponentModel.DataAnnotations;
using DesignAuthority.Application.Abstractions;

namespace DesignAuthority.Web.ViewModels.Findings;

public sealed class FindingViewModel
{
    [Required, DataType(DataType.Date)]
    public DateOnly RegistrationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(1, int.MaxValue)]
    public int TramiteId { get; set; }

    [Range(1, int.MaxValue)]
    public int SubstageId { get; set; }

    [Range(1, int.MaxValue)]
    public int TechnologyId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ResponsiblePersonId { get; set; }

    [Range(1, int.MaxValue)]
    public int StatusId { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int CriticalityId { get; set; }

    [Range(1, int.MaxValue)]
    public int PriorityId { get; set; }

    [Range(1, int.MaxValue)]
    public int RegistrationPhaseId { get; set; }

    [Range(1, int.MaxValue)]
    public int AttentionPhaseId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly CommitmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Date)]
    public DateOnly? SolutionDate { get; set; }

    public WorkItemFormOptions? Options { get; set; }

    public FindingInput ToInput() => new(
        RegistrationDate, TramiteId, SubstageId, TechnologyId, CategoryId, Description,
        ResponsiblePersonId, StatusId, CriticalityId, PriorityId, RegistrationPhaseId,
        AttentionPhaseId, CommitmentDate, SolutionDate);

    public static FindingViewModel FromInput(FindingInput input, WorkItemFormOptions options) => new()
    {
        RegistrationDate = input.RegistrationDate,
        TramiteId = input.TramiteId,
        SubstageId = input.SubstageId,
        TechnologyId = input.TechnologyId,
        CategoryId = input.CategoryId,
        Description = input.Description,
        ResponsiblePersonId = input.ResponsiblePersonId,
        StatusId = input.StatusId,
        CriticalityId = input.CriticalityId,
        PriorityId = input.PriorityId,
        RegistrationPhaseId = input.RegistrationPhaseId,
        AttentionPhaseId = input.AttentionPhaseId,
        CommitmentDate = input.CommitmentDate,
        SolutionDate = input.SolutionDate,
        Options = options
    };
}
