namespace Audit.Contracts.Dtos;

/// <summary>
/// DTO representing an audit log entry for API responses.
/// </summary>
public sealed record AuditLogDto(
    Guid Id,
    DateTime Timestamp,
    string UserEmail,
    string EntityName,
    string EntityId,
    string Action,
    IReadOnlyList<AuditChangeDto> Changes);

/// <summary>
/// Represents a single field-level change within an audit log entry.
/// </summary>
public sealed record AuditChangeDto(
    string PropertyName,
    string? OldValue,
    string? NewValue);
