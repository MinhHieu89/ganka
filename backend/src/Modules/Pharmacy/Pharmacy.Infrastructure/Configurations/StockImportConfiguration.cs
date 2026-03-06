using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for StockImport and StockImportLine.
/// StockImport represents a single stock import event (supplier invoice or Excel bulk import).
/// StockImportLine is configured inline via the owned navigation relationship.
/// </summary>
public class StockImportConfiguration : IEntityTypeConfiguration<StockImport>
{
    public void Configure(EntityTypeBuilder<StockImport> builder)
    {
        builder.ToTable("StockImports");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SupplierId)
            .IsRequired();

        builder.Property(s => s.SupplierName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ImportSource)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.InvoiceNumber)
            .HasMaxLength(100);

        builder.Property(s => s.ImportedById)
            .IsRequired();

        builder.Property(s => s.ImportedAt)
            .IsRequired();

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Configure backing field access for the Lines collection
        builder.Navigation(s => s.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Configure one-to-many: StockImport -> StockImportLines
        builder.HasMany(s => s.Lines)
            .WithOne()
            .HasForeignKey(l => l.StockImportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core entity configuration for StockImportLine.
/// Configures the "StockImportLines" table in the pharmacy schema.
/// StockImportLine is a child entity — always created via StockImport.AddLine().
/// </summary>
public class StockImportLineConfiguration : IEntityTypeConfiguration<StockImportLine>
{
    public void Configure(EntityTypeBuilder<StockImportLine> builder)
    {
        builder.ToTable("StockImportLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.StockImportId)
            .IsRequired();

        builder.Property(l => l.DrugCatalogItemId)
            .IsRequired();

        builder.Property(l => l.DrugName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.BatchNumber)
            .IsRequired()
            .HasMaxLength(100);

        // DateOnly maps natively in EF Core 8+ (no converter needed)
        builder.Property(l => l.ExpiryDate)
            .IsRequired();

        builder.Property(l => l.Quantity)
            .IsRequired();

        builder.Property(l => l.PurchasePrice)
            .IsRequired()
            .HasPrecision(18, 2);
    }
}
