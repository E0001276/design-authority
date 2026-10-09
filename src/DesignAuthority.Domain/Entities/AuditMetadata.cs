namespace DesignAuthority.Domain.Entities;

public sealed record AuditMetadata(
    DateTime CreatedAtUtc,
    int CreatedByUserId,
    DateTime? UpdatedAtUtc,
    int? UpdatedByUserId,
    bool IsActive,
    DateTime? DeletedAtUtc,
    int? DeletedByUserId);
