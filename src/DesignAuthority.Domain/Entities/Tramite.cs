namespace DesignAuthority.Domain.Entities;

public sealed record Tramite
{
    public Tramite(int id, string name, string? description, bool isDefault, bool isActive)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Id = id;
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsDefault = isDefault;
        IsActive = isActive;
    }

    public int Id { get; }
    public string Name { get; }
    public string? Description { get; }
    public bool IsDefault { get; }
    public bool IsActive { get; }
}

public sealed record Substage
{
    public Substage(int id, int tramiteId, string name, string? description, bool isActive)
    {
        if (id <= 0 || tramiteId <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Id = id;
        TramiteId = tramiteId;
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = isActive;
    }

    public int Id { get; }
    public int TramiteId { get; }
    public string Name { get; }
    public string? Description { get; }
    public bool IsActive { get; }
}
