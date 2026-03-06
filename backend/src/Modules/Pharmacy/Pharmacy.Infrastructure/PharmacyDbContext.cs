using Microsoft.EntityFrameworkCore;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure;

/// <summary>
/// EF Core DbContext for the Pharmacy module.
/// Uses schema-per-module isolation with the "pharmacy" schema.
/// </summary>
public class PharmacyDbContext : DbContext
{
    public DbSet<DrugCatalogItem> DrugCatalogItems => Set<DrugCatalogItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierDrugPrice> SupplierDrugPrices => Set<SupplierDrugPrice>();
    public DbSet<DrugBatch> DrugBatches => Set<DrugBatch>();
    public DbSet<StockImport> StockImports => Set<StockImport>();
    public DbSet<StockImportLine> StockImportLines => Set<StockImportLine>();

    // Dispensing (PHR-05)
    public DbSet<DispensingRecord> DispensingRecords => Set<DispensingRecord>();
    public DbSet<DispensingLine> DispensingLines => Set<DispensingLine>();
    public DbSet<BatchDeduction> BatchDeductions => Set<BatchDeduction>();

    // OTC Sales (PHR-06)
    public DbSet<OtcSale> OtcSales => Set<OtcSale>();
    public DbSet<OtcSaleLine> OtcSaleLines => Set<OtcSaleLine>();

    // Consumables warehouse (CON-01, CON-02)
    public DbSet<ConsumableItem> ConsumableItems => Set<ConsumableItem>();
    public DbSet<ConsumableBatch> ConsumableBatches => Set<ConsumableBatch>();

    // Stock adjustments (shared between pharmacy and consumables)
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("pharmacy");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PharmacyDbContext).Assembly);

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
