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

        base.OnModelCreating(modelBuilder);
    }
}
