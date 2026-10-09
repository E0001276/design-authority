namespace DesignAuthority.Domain.Entities;

public sealed record MeetingMinute
{
    public MeetingMinute(
        int meetingId,
        string originalFileName,
        string storageFileName,
        string relativePath,
        AuditMetadata audit)
    {
        if (meetingId <= 0) throw new ArgumentOutOfRangeException(nameof(meetingId));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name is required.", nameof(originalFileName));
        if (string.IsNullOrWhiteSpace(storageFileName))
            throw new ArgumentException("Storage file name is required.", nameof(storageFileName));
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new ArgumentException("A relative file reference is required.", nameof(relativePath));
        MeetingId = meetingId;
        OriginalFileName = originalFileName;
        StorageFileName = storageFileName;
        RelativePath = relativePath;
        Audit = audit ?? throw new ArgumentNullException(nameof(audit));
    }

    public int MeetingId { get; }
    public string OriginalFileName { get; }
    public string StorageFileName { get; }
    public string RelativePath { get; }
    public AuditMetadata Audit { get; }
}
