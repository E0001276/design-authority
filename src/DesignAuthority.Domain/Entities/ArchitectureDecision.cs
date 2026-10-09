namespace DesignAuthority.Domain.Entities;

public sealed record ArchitectureDecision
{
    public ArchitectureDecision(
        string id,
        int tramiteId,
        int substageId,
        int technologyId,
        int responsibleTeamTechnologyId,
        string problemDescription,
        string solutionApproach,
        string decisionText,
        DateOnly registrationDate,
        int statusId,
        AuditMetadata audit)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (tramiteId <= 0 || substageId <= 0 || technologyId <= 0 || responsibleTeamTechnologyId <= 0 || statusId <= 0)
            throw new ArgumentOutOfRangeException(nameof(tramiteId), "Required references must be positive.");
        if (string.IsNullOrWhiteSpace(problemDescription)) throw new ArgumentException("Problem description is required.", nameof(problemDescription));
        if (string.IsNullOrWhiteSpace(solutionApproach)) throw new ArgumentException("Solution approach is required.", nameof(solutionApproach));
        if (string.IsNullOrWhiteSpace(decisionText)) throw new ArgumentException("Decision text is required.", nameof(decisionText));
        if (registrationDate == default) throw new ArgumentException("Registration date is required.", nameof(registrationDate));

        Id = id;
        TramiteId = tramiteId;
        SubstageId = substageId;
        TechnologyId = technologyId;
        ResponsibleTeamTechnologyId = responsibleTeamTechnologyId;
        ProblemDescription = problemDescription.Trim();
        SolutionApproach = solutionApproach.Trim();
        DecisionText = decisionText.Trim();
        RegistrationDate = registrationDate;
        StatusId = statusId;
        Audit = audit ?? throw new ArgumentNullException(nameof(audit));
    }

    public string Id { get; }
    public int TramiteId { get; }
    public int SubstageId { get; }
    public int TechnologyId { get; }
    public int ResponsibleTeamTechnologyId { get; }
    public string ProblemDescription { get; }
    public string SolutionApproach { get; }
    public string DecisionText { get; }
    public DateOnly RegistrationDate { get; }
    public int StatusId { get; }
    public AuditMetadata Audit { get; }
}
