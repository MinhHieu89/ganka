using Audit.Application;
using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure;

/// <summary>
/// EF Core DbContext for the Audit module.
/// Uses schema-per-module isolation with the "audit" schema.
/// NO global query filters -- audit logs are immutable and must be queryable across all branches.
/// Implements IAuditReadContext for clean Application-layer query access.
/// </summary>
public class AuditDbContext : DbContext, IAuditReadContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    // Explicit interface implementation for IQueryable access
    IQueryable<AuditLog> IAuditReadContext.AuditLogs => AuditLogs.AsNoTracking();
    IQueryable<AccessLog> IAuditReadContext.AccessLogs => AccessLogs.AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
