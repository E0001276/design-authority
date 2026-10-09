namespace DesignAuthority.Domain.Entities;

public sealed record Agreement
{
    public Agreement(
        int id,
        DateOnly registrationDate,
        int tramiteId,
        int technologyId,
        string description,
        int responsiblePersonId,
        int statusId,
        DateOnly commitmentDate,
        DateOnly? solutionDate,
        AuditMetadata audit)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (tramiteId <= 0 || technologyId <= 0 || responsiblePersonId <= 0 || statusId <= 0)
            throw new ArgumentOutOfRangeException(nameof(tramiteId), "Required references must be positive.");
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (registrationDate == default) throw new ArgumentException("Registration date is required.", nameof(registrationDate));
        if (commitmentDate == default) throw new ArgumentException("Commitment date is required.", nameof(commitmentDate));

        Id = id;
        RegistrationDate = registrationDate;
        TramiteId = tramiteId;
        TechnologyId = technologyId;
        Description = description.Trim();
        ResponsiblePersonId = responsiblePersonId;
        StatusId = statusId;
        CommitmentDate = commitmentDate;
        SolutionDate = solutionDate;
        Audit = audit ?? throw new ArgumentNullException(nameof(audit));
    }

    public int Id { get; }
    public DateOnly RegistrationDate { get; }
    public int TramiteId { get; }
    public int TechnologyId { get; }
    public string Description { get; }
    public int ResponsiblePersonId { get; }
    public int StatusId { get; }
    public DateOnly CommitmentDate { get; }
    public DateOnly? SolutionDate { get; }
    public AuditMetadata Audit { get; }
}

public sealed record AgreementFollowUp
{
    public AgreementFollowUp(
        int id,
        int agreementId,
        DateOnly date,
        int userId,
        string observation,
        int? oldStatusId,
        int? newStatusId,
        DateOnly? oldCommitmentDate,
        DateOnly? newCommitmentDate)
    {
        if (id <= 0 || agreementId <= 0 || userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Required identifiers must be positive.");
        if (date == default) throw new ArgumentException("Follow-up date is required.", nameof(date));
        if (string.IsNullOrWhiteSpace(observation))
            throw new ArgumentException("Observation is required.", nameof(observation));
        if (oldStatusId.HasValue != newStatusId.HasValue)
            throw new ArgumentException("Status changes require both old and new values.");
        if (oldCommitmentDate.HasValue != newCommitmentDate.HasValue)
            throw new ArgumentException("Commitment date changes require both old and new values.");
        Id = id;
        AgreementId = agreementId;
        Date = date;
        UserId = userId;
        Observation = observation.Trim();
        OldStatusId = oldStatusId;
        NewStatusId = newStatusId;
        OldCommitmentDate = oldCommitmentDate;
        NewCommitmentDate = newCommitmentDate;
    }

    public int Id { get; }
    public int AgreementId { get; }
    public DateOnly Date { get; }
    public int UserId { get; }
    public string Observation { get; }
    public int? OldStatusId { get; }
    public int? NewStatusId { get; }
    public DateOnly? OldCommitmentDate { get; }
    public DateOnly? NewCommitmentDate { get; }
}
