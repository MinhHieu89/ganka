using Microsoft.EntityFrameworkCore;

namespace Clinical.Infrastructure;

/// <summary>
/// EF Core DbContext for the Clinical module.
/// Uses schema-per-module isolation with the "clinical" schema.
/// Entity configurations and DbSets will be added as the module is implemented.
/// </summary>
public class ClinicalDbContext : DbContext
{
    public ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("clinical");

        // Entity configurations will be added as this module is implemented
        // in its respective phase plan.

        base.OnModelCreating(modelBuilder);
    }
}
