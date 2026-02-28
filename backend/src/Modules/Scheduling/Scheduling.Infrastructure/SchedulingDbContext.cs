using Microsoft.EntityFrameworkCore;

namespace Scheduling.Infrastructure;

/// <summary>
/// EF Core DbContext for the Scheduling module.
/// Uses schema-per-module isolation with the "scheduling" schema.
/// Entity configurations and DbSets will be added as the module is implemented.
/// </summary>
public class SchedulingDbContext : DbContext
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("scheduling");

        // Entity configurations will be added as this module is implemented
        // in its respective phase plan.

        base.OnModelCreating(modelBuilder);
    }
}
