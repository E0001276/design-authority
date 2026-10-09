using DesignAuthority.Domain.Entities;

namespace DesignAuthority.Domain.Tests.Decisions;

public sealed class ArchitectureDecisionTests
{
    [Fact]
    public void Decision_identifier_is_immutable_and_required_associations_are_retained()
    {
        var decision = new ArchitectureDecision(
            "TRAMITE1-SUB1-001", 1, 2, 3, 5, "problem", "approach", "decision",
            DateOnly.FromDateTime(DateTime.UtcNow), 1,
            new AuditMetadata(DateTime.UtcNow, 1, null, null, true, null, null));

        Assert.Equal("TRAMITE1-SUB1-001", decision.Id);
        Assert.Equal(1, decision.TramiteId);
        Assert.Equal(2, decision.SubstageId);
        Assert.Null(typeof(ArchitectureDecision).GetProperty(nameof(ArchitectureDecision.Id))!.SetMethod);
    }
}
