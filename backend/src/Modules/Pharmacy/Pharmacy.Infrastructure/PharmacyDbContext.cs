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
