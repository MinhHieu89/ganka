using Microsoft.EntityFrameworkCore;

namespace Pharmacy.Infrastructure;

/// <summary>
/// EF Core DbContext for the Pharmacy module.
/// Uses schema-per-module isolation with the "pharmacy" schema.
/// Entity configurations and DbSets will be added as the module is implemented.
/// </summary>
public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("pharmacy");

        // Entity configurations will be added as this module is implemented
        // in its respective phase plan.

        base.OnModelCreating(modelBuilder);
    }
}
