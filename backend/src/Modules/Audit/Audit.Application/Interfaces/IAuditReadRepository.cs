using Audit.Domain.Entities;

namespace Audit.Application.Interfaces;

/// <summary>
/// Read-only query interface for the Audit module's data.
/// Used by feature handlers and endpoints to query audit/access logs
/// without creating a circular dependency with the Infrastructure layer.
/// Implemented by AuditDbContext in Audit.Infrastructure.
/// </summary>
public interface IAuditReadRepository
{
    IQueryable<AuditLog> AuditLogs { get; }
    IQueryable<AccessLog> AccessLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
