namespace DesignAuthority.Domain.Entities;

public sealed record Meeting
{
    public Meeting(
        int id,
        int tramiteId,
        int technologyId,
        DateOnly date,
        TimeOnly time,
        int statusId,
        IReadOnlyCollection<MeetingParticipant> participants,
        string agreementsSummary,
        string commitmentsSummary,
        string subject,
        string? comments,
        AuditMetadata audit)
    {
        if (id <= 0 || tramiteId <= 0 || technologyId <= 0 || statusId <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Required identifiers must be positive.");
        if (date == default) throw new ArgumentException("Meeting date is required.", nameof(date));
        ArgumentNullException.ThrowIfNull(participants);
        if (participants.Count == 0 || !participants.Any(participant => participant.Attended))
            throw new ArgumentException("At least one attendee is required.", nameof(participants));
        if (participants.Any(participant => participant.MeetingId != id || participant.PersonId <= 0)
            || participants.Select(participant => participant.PersonId).Distinct().Count() != participants.Count)
            throw new ArgumentException("Participants must be unique and belong to this meeting.", nameof(participants));
        if (string.IsNullOrWhiteSpace(agreementsSummary)) throw new ArgumentException("Agreements summary is required.", nameof(agreementsSummary));
        if (string.IsNullOrWhiteSpace(commitmentsSummary)) throw new ArgumentException("Commitments summary is required.", nameof(commitmentsSummary));
        if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject is required.", nameof(subject));

        Id = id;
        TramiteId = tramiteId;
        TechnologyId = technologyId;
        Date = date;
        Time = time;
        StatusId = statusId;
        Participants = participants.ToArray();
        AgreementsSummary = agreementsSummary.Trim();
        CommitmentsSummary = commitmentsSummary.Trim();
        Subject = subject.Trim();
        Comments = comments;
        Audit = audit ?? throw new ArgumentNullException(nameof(audit));
    }

    public int Id { get; }
    public int TramiteId { get; }
    public int TechnologyId { get; }
    public DateOnly Date { get; }
    public TimeOnly Time { get; }
    public int StatusId { get; }
    public IReadOnlyCollection<MeetingParticipant> Participants { get; }
    public string AgreementsSummary { get; }
    public string CommitmentsSummary { get; }
    public string Subject { get; }
    public string? Comments { get; }
    public AuditMetadata Audit { get; }
}

public sealed record MeetingParticipant(int MeetingId, int PersonId, bool Attended);
