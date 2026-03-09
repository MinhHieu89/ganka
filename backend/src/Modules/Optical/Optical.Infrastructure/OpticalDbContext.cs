using Microsoft.EntityFrameworkCore;
using Optical.Domain.Entities;
using Shared.Infrastructure;

namespace Optical.Infrastructure;

/// <summary>
/// EF Core DbContext for the Optical module.
/// Uses schema-per-module isolation with the "optical" schema.
/// Applies all entity configurations via ApplyConfigurationsFromAssembly.
/// </summary>
public class OpticalDbContext : DbContext
{
    // Frame inventory (OPT-01)
    public DbSet<Frame> Frames => Set<Frame>();

    // Lens catalog with per-power stock entries (OPT-02)
    public DbSet<LensCatalogItem> LensCatalogItems => Set<LensCatalogItem>();
    public DbSet<LensStockEntry> LensStockEntries => Set<LensStockEntry>();
    public DbSet<LensOrder> LensOrders => Set<LensOrder>();

    // Glasses order lifecycle (OPT-03)
    public DbSet<GlassesOrder> GlassesOrders => Set<GlassesOrder>();
    public DbSet<GlassesOrderItem> GlassesOrderItems => Set<GlassesOrderItem>();

    // Combo packages (OPT-06)
    public DbSet<ComboPackage> ComboPackages => Set<ComboPackage>();

    // Warranty claims (OPT-07)
    public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();

    // Stocktaking (OPT-09)
    public DbSet<StocktakingSession> StocktakingSessions => Set<StocktakingSession>();
    public DbSet<StocktakingItem> StocktakingItems => Set<StocktakingItem>();

    public OpticalDbContext(DbContextOptions<OpticalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("optical");

        // Auto-discover all IEntityTypeConfiguration implementations in this assembly.
        // Configurations are added as entities are implemented in their respective plan files.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpticalDbContext).Assembly);

        modelBuilder.ApplySharedConventions();

        base.OnModelCreating(modelBuilder);
    }
}
