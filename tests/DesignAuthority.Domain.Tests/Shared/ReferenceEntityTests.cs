using DesignAuthority.Domain.Entities;

namespace DesignAuthority.Domain.Tests.Shared;

public sealed class ReferenceEntityTests
{
    [Fact]
    public void Team_technology_kind_and_reference_properties_are_immutable()
    {
        var item = new TeamTechnology(1, "APPIAN", "Appian", TeamTechnologyKind.Technology);

        Assert.Equal(TeamTechnologyKind.Technology, item.Kind);
        Assert.Null(typeof(TeamTechnology).GetProperty(nameof(TeamTechnology.Kind))!.SetMethod);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TeamTechnology(1, "APPIAN", "Appian", (TeamTechnologyKind)99));
    }

    [Fact]
    public void Person_requires_one_catalog_assignment_and_has_an_immutable_id()
    {
        var person = new Person(1, "Ana", "García", null, "AG", 2, 3,
            DateTime.UtcNow, 1);

        Assert.Equal(2, person.TeamTechnologyId);
        Assert.Equal(3, person.RoleId);
        Assert.Null(typeof(Person).GetProperty(nameof(Person.Id))!.SetMethod);
        Assert.Throws<ArgumentException>(() =>
            new Person(1, "Ana", "García", null, "AG", 2, 3, DateTime.UtcNow, 1,
                isActive: false));
    }

    [Fact]
    public void Tramite_and_substage_require_names_and_positive_identifiers()
    {
        var tramite = new Tramite(1, "Canónico", null, isDefault: false, isActive: true);
        var substage = new Substage(2, tramite.Id, "Diseño", null, isActive: true);

        Assert.Equal(tramite.Id, substage.TramiteId);
        Assert.Throws<ArgumentException>(() => new Tramite(1, " ", null, false, true));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Substage(0, 1, "Diseño", null, true));
    }
}
