namespace DesignAuthority.Domain.Entities;

public enum TeamTechnologyKind
{
    Technology,
    Team
}

public sealed record TeamTechnology
{
    public TeamTechnology(int id, string code, string name, TeamTechnologyKind kind)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        Id = id;
        Code = code.Trim();
        Name = name.Trim();
        Kind = kind;
    }

    public int Id { get; }
    public string Code { get; }
    public string Name { get; }
    public TeamTechnologyKind Kind { get; }
}

public sealed record Role
{
    public Role(int id, int teamTechnologyId, string code, string name, bool isLeadership)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (teamTechnologyId <= 0) throw new ArgumentOutOfRangeException(nameof(teamTechnologyId));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Id = id;
        TeamTechnologyId = teamTechnologyId;
        Code = code.Trim();
        Name = name.Trim();
        IsLeadership = isLeadership;
    }

    public int Id { get; }
    public int TeamTechnologyId { get; }
    public string Code { get; }
    public string Name { get; }
    public bool IsLeadership { get; }
}
