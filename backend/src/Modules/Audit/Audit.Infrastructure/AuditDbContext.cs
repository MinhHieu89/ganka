using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure;

/// <summary>
/// EF Core DbContext for the Audit module.
/// Uses schema-per-module isolation with the "audit" schema.
/// Entity configurations will be added in plan 01-04 (Audit interceptor and access logging).
/// </summary>
public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit");

        // Audit entity configurations (AuditLogs, AccessLogs)
        // will be added in plan 01-04.

        base.OnModelCreating(modelBuilder);
    }
}
