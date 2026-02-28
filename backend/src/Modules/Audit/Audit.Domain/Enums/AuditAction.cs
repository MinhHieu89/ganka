namespace Audit.Domain.Enums;

/// <summary>
/// Types of entity-level changes captured by the audit interceptor.
/// </summary>
public enum AuditAction
{
    Created = 0,
    Updated = 1,
    Deleted = 2
}
