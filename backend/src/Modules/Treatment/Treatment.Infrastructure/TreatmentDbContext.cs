using Microsoft.EntityFrameworkCore;

namespace Treatment.Infrastructure;

/// <summary>
/// EF Core DbContext for the Treatment module.
/// Uses schema-per-module isolation with the "treatment" schema.
/// Entity configurations and DbSets will be added as the module is implemented.
/// </summary>
public class TreatmentDbContext : DbContext
{
    public TreatmentDbContext(DbContextOptions<TreatmentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("treatment");

        // Entity configurations will be added as this module is implemented
        // in its respective phase plan.

        base.OnModelCreating(modelBuilder);
    }
}
