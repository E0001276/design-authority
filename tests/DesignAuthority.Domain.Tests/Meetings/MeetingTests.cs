using DesignAuthority.Domain.Entities;

namespace DesignAuthority.Domain.Tests.Meetings;

public sealed class MeetingTests
{
    [Fact]
    public void Meeting_requires_an_attendee_and_has_no_substage_association()
    {
        var audit = new AuditMetadata(DateTime.UtcNow, 1, null, null, true, null, null);
        var participant = new MeetingParticipant(7, 3, true);
        var meeting = new Meeting(
            7, 1, 2, DateOnly.FromDateTime(DateTime.UtcNow), new TimeOnly(10, 30), 1,
            [participant], "agreements", "commitments", "subject", null, audit);

        Assert.Equal(7, meeting.Id);
        Assert.Null(typeof(Meeting).GetProperty(nameof(Meeting.Id))!.SetMethod);
        Assert.DoesNotContain(typeof(Meeting).GetProperties(), property => property.Name.Contains("Substage", StringComparison.Ordinal));
        Assert.Throws<ArgumentException>(() => new Meeting(
            8, 1, 2, DateOnly.FromDateTime(DateTime.UtcNow), new TimeOnly(10, 30), 1,
            [], "agreements", "commitments", "subject", null, audit));
    }

    [Fact]
    public void Meeting_minute_retains_only_a_relative_current_file_reference()
    {
        var audit = new AuditMetadata(DateTime.UtcNow, 1, null, null, true, null, null);

        var minute = new MeetingMinute(7, "review.docx", "a1.docx", "minutes/2026/a1.docx", audit);

        Assert.Equal("review.docx", minute.OriginalFileName);
        Assert.Throws<ArgumentException>(() =>
            new MeetingMinute(7, "review.docx", "a1.docx", Path.GetFullPath("a1.docx"), audit));
    }
}
