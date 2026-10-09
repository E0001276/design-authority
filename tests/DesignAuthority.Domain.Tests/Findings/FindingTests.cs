using System.Reflection;
using DesignAuthority.Domain.Entities;

namespace DesignAuthority.Domain.Tests.Findings;

public sealed class FindingTests
{
    [Fact]
    public void Finding_requires_positive_identifiers_and_preserves_immutable_id()
    {
        var finding = CreateFinding();

        Assert.Equal(12, finding.Id);
        Assert.Null(typeof(Finding).GetProperty(nameof(Finding.Id))!.SetMethod);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Finding(
            0, DateOnly.FromDateTime(DateTime.UtcNow), 1, 2, 3, 4, "description",
            5, 1, 1, 1, 1, 1, DateOnly.FromDateTime(DateTime.UtcNow), null, Audit()));
    }

    [Fact]
    public void Finding_follow_up_preserves_each_observation_and_paired_changes()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var followUp = new FindingFollowUp(1, 12, date, 1, " Observación ", 1, 2,
            date, date.AddDays(1));

        Assert.Equal("Observación", followUp.Observation);
        Assert.Equal(1, followUp.OldStatusId);
        Assert.Equal(2, followUp.NewStatusId);
        Assert.Equal(date.AddDays(1), followUp.NewCommitmentDate);
        Assert.Throws<ArgumentException>(() =>
            new FindingFollowUp(2, 12, date, 1, "observación", 1, null, null, null));
    }

    private static Finding CreateFinding() => new(
        12, DateOnly.FromDateTime(DateTime.UtcNow), 1, 2, 3, 4, "description",
        5, 1, 1, 1, 1, 1, DateOnly.FromDateTime(DateTime.UtcNow), null, Audit());

    private static AuditMetadata Audit() => new(DateTime.UtcNow, 1, null, null, true, null, null);
}
