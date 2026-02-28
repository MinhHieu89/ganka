using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure;

/// <summary>
/// EF Core DbContext for the Billing module.
/// Uses schema-per-module isolation with the "billing" schema.
/// Entity configurations and DbSets will be added as the module is implemented.
/// </summary>
public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");

        // Entity configurations will be added as this module is implemented
        // in its respective phase plan.

        base.OnModelCreating(modelBuilder);
    }
}
