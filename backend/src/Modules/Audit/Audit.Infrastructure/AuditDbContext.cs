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

        // All domain entities generate their own Guid IDs in the constructor (client-side).
        // Override EF Core's default ValueGeneratedOnAdd to prevent it from treating
        // new entities with set IDs as existing (Modified) instead of new (Added).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty is not null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
