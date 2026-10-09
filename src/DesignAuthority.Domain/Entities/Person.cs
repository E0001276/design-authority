namespace DesignAuthority.Domain.Entities;

public sealed class Person
{
    public Person(
        int id,
        string name,
        string paternalSurname,
        string? maternalSurname,
        string abbreviatedName,
        int teamTechnologyId,
        int roleId,
        DateTime createdAtUtc,
        int createdByUserId,
        bool isActive = true,
        DateTime? updatedAtUtc = null,
        int? updatedByUserId = null,
        DateTime? deletedAtUtc = null,
        int? deletedByUserId = null)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(paternalSurname)) throw new ArgumentException("Paternal surname is required.", nameof(paternalSurname));
        if (string.IsNullOrWhiteSpace(abbreviatedName)) throw new ArgumentException("Abbreviated name is required.", nameof(abbreviatedName));
        if (teamTechnologyId <= 0) throw new ArgumentOutOfRangeException(nameof(teamTechnologyId));
        if (roleId <= 0) throw new ArgumentOutOfRangeException(nameof(roleId));
        if (createdByUserId <= 0) throw new ArgumentOutOfRangeException(nameof(createdByUserId));
        if (isActive && (deletedAtUtc is not null || deletedByUserId is not null))
            throw new ArgumentException("An active person cannot have deletion audit values.");
        if (!isActive && (deletedAtUtc is null || deletedByUserId is null))
            throw new ArgumentException("A deleted person requires deletion audit values.");

        Id = id;
        Name = name.Trim();
        PaternalSurname = paternalSurname.Trim();
        MaternalSurname = string.IsNullOrWhiteSpace(maternalSurname) ? null : maternalSurname.Trim();
        AbbreviatedName = abbreviatedName.Trim();
        TeamTechnologyId = teamTechnologyId;
        RoleId = roleId;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
        UpdatedByUserId = updatedByUserId;
        DeletedAtUtc = deletedAtUtc;
        DeletedByUserId = deletedByUserId;
    }

    public int Id { get; }
    public string Name { get; }
    public string PaternalSurname { get; }
    public string? MaternalSurname { get; }
    public string AbbreviatedName { get; }
    public int TeamTechnologyId { get; }
    public int RoleId { get; }
    public DateTime CreatedAtUtc { get; }
    public int CreatedByUserId { get; }
    public bool IsActive { get; }
    public DateTime? UpdatedAtUtc { get; }
    public int? UpdatedByUserId { get; }
    public DateTime? DeletedAtUtc { get; }
    public int? DeletedByUserId { get; }
}
