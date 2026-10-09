using System.ComponentModel.DataAnnotations;
using DesignAuthority.Application.Abstractions;

namespace DesignAuthority.Web.ViewModels.Agreements;

public sealed class AgreementViewModel
{
    [Required, DataType(DataType.Date)]
    public DateOnly RegistrationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Range(1, int.MaxValue)]
    public int TramiteId { get; set; }
    [Range(1, int.MaxValue)]
    public int TechnologyId { get; set; }
    [Required]
    public string Description { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int ResponsiblePersonId { get; set; }
    [Range(1, int.MaxValue)]
    public int StatusId { get; set; } = 1;
    [Required, DataType(DataType.Date)]
    public DateOnly CommitmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [DataType(DataType.Date)]
    public DateOnly? SolutionDate { get; set; }
    public WorkItemFormOptions? Options { get; set; }

    public AgreementInput ToInput() => new(RegistrationDate, TramiteId, TechnologyId,
        Description, ResponsiblePersonId, StatusId, CommitmentDate, SolutionDate);

    public static AgreementViewModel FromInput(AgreementInput input, WorkItemFormOptions options) => new()
    {
        RegistrationDate = input.RegistrationDate,
        TramiteId = input.TramiteId,
        TechnologyId = input.TechnologyId,
        Description = input.Description,
        ResponsiblePersonId = input.ResponsiblePersonId,
        StatusId = input.StatusId,
        CommitmentDate = input.CommitmentDate,
        SolutionDate = input.SolutionDate,
        Options = options
    };
}
