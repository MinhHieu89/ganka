using Audit.Domain.Enums;

namespace Audit.Domain.Entities;

/// <summary>
/// Immutable audit log entry capturing field-level changes on IAuditable entities.
/// NOT an AggregateRoot -- standalone entity with no BranchId filtering.
/// Audit logs are append-only: no UPDATE or DELETE operations allowed.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public AuditAction Action { get; private set; }

    /// <summary>
    /// JSON serialized list of { PropertyName, OldValue, NewValue } change entries.
    /// </summary>
    public string Changes { get; private set; } = string.Empty;

    /// <summary>
    /// Stored for reference but NOT used as a query filter.
    /// Audit logs are cross-tenant immutable records.
    /// </summary>
    public Guid BranchId { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid userId,
        string userEmail,
        string entityName,
        string entityId,
        AuditAction action,
        string changes,
        Guid branchId)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            UserEmail = userEmail,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            Changes = changes,
            BranchId = branchId
        };
    }
}
