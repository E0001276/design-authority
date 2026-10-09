namespace DesignAuthority.Domain.Entities;

public sealed record ConfiguredUser
{
    public ConfiguredUser(int id, string displayName)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));
        Id = id;
        DisplayName = displayName.Trim();
    }

    public int Id { get; }
    public string DisplayName { get; }
}
