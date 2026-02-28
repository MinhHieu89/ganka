using Microsoft.EntityFrameworkCore;

namespace Optical.Infrastructure;

/// <summary>
/// EF Core DbContext for the Optical module.
/// Uses schema-per-module isolation with the "optical" schema.
/// Entity configurations and DbSets will be added as the module is implemented.
/// </summary>
public class OpticalDbContext : DbContext
{
    public OpticalDbContext(DbContextOptions<OpticalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("optical");

        // Entity configurations will be added as this module is implemented
        // in its respective phase plan.

        base.OnModelCreating(modelBuilder);
    }
}
