using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure;

/// <summary>
/// EF Core DbContext for the Audit module.
/// Uses schema-per-module isolation with the "audit" schema.
/// NO global query filters -- audit logs are immutable and must be queryable across all branches.
/// Implements IAuditReadRepository for clean Application-layer query access.
/// </summary>
public class AuditDbContext : DbContext, IAuditReadRepository
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    // Explicit interface implementation for IQueryable access
    IQueryable<AuditLog> IAuditReadRepository.AuditLogs => AuditLogs.AsNoTracking();
    IQueryable<AccessLog> IAuditReadRepository.AccessLogs => AccessLogs.AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
