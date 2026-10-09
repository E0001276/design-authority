using DesignAuthority.Domain.Entities;

namespace DesignAuthority.Domain.Tests.Agreements;

public sealed class AgreementTests
{
    [Fact]
    public void Agreement_has_no_substage_and_its_identifier_is_immutable()
    {
        var agreement = new Agreement(
            5, DateOnly.FromDateTime(DateTime.UtcNow), 1, 2, "description",
            3, 1, DateOnly.FromDateTime(DateTime.UtcNow), null,
            new AuditMetadata(DateTime.UtcNow, 1, null, null, true, null, null));

        Assert.Equal(5, agreement.Id);
        Assert.Null(typeof(Agreement).GetProperty(nameof(Agreement.Id))!.SetMethod);
        Assert.DoesNotContain(typeof(Agreement).GetProperties(), property => property.Name.Contains("Substage", StringComparison.Ordinal));
    }
}
