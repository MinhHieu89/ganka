namespace Audit.Contracts.Dtos;

/// <summary>
/// Query parameters for filtering audit log entries.
/// Supports cursor-based pagination using Timestamp + Id.
/// </summary>
public sealed record AuditLogQuery(
    Guid? UserId = null,
    string? ActionType = null,
    string? EntityName = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    DateTime? CursorTimestamp = null,
    Guid? CursorId = null,
    int PageSize = 50);
