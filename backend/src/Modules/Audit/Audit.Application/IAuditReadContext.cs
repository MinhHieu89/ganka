using Audit.Domain.Entities;

namespace Audit.Application;

/// <summary>
/// Read-only query interface for the Audit module's data.
/// Used by endpoints in the Application layer to query audit/access logs
/// without creating a circular dependency with the Infrastructure layer.
/// Implemented by AuditDbContext in Audit.Infrastructure.
/// </summary>
public interface IAuditReadContext
{
    IQueryable<AuditLog> AuditLogs { get; }
    IQueryable<AccessLog> AccessLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
