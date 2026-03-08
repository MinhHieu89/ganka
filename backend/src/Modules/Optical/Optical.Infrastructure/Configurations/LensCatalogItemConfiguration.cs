using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optical.Domain.Entities;

namespace Optical.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for LensCatalogItem and its child LensStockEntry entities.
/// Maps the lens catalog aggregate root to "LensCatalogItems" and child stock entries
/// to "LensStockEntries" in the optical schema.
/// The StockEntries collection uses a backing field (_stockEntries) per DDD encapsulation.
/// </summary>
public class LensCatalogItemConfiguration : IEntityTypeConfiguration<LensCatalogItem>
{
    public void Configure(EntityTypeBuilder<LensCatalogItem> builder)
    {
        builder.ToTable("LensCatalogItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LensType)
            .IsRequired()
            .HasMaxLength(50);

        // LensMaterial enum stored as int
        builder.Property(x => x.Material)
            .IsRequired()
            .HasConversion<int>();

        // LensCoating [Flags] enum stored as combined int value
        builder.Property(x => x.AvailableCoatings)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.SellingPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.CostPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Nullable: cross-module reference to Pharmacy.Supplier by ID
        builder.Property(x => x.PreferredSupplierId)
            .IsRequired(false);

        // BranchId value object stored as Guid column
        builder.Property(x => x.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Configure backing field access for the StockEntries collection.
        // LensCatalogItem exposes StockEntries as IReadOnlyList<LensStockEntry>
        // but stores them in the private _stockEntries backing field.
        builder.Navigation(x => x.StockEntries)
            .HasField("_stockEntries")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: LensCatalogItem -> LensStockEntries
        builder.HasMany(x => x.StockEntries)
            .WithOne()
            .HasForeignKey(e => e.LensCatalogItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Search performance indexes
        builder.HasIndex(x => x.Brand);
        builder.HasIndex(x => x.IsActive);
    }
}

/// <summary>
/// EF Core entity configuration for LensStockEntry.
/// Maps lens stock entries for specific power combinations (SPH/CYL/ADD) to "LensStockEntries".
/// A unique composite index on (LensCatalogItemId, Sph, Cyl, Add) prevents duplicate power entries
/// for the same catalog item — preventing data integrity issues during stocktaking (pitfall 5).
/// </summary>
public class LensStockEntryConfiguration : IEntityTypeConfiguration<LensStockEntry>
{
    public void Configure(EntityTypeBuilder<LensStockEntry> builder)
    {
        builder.ToTable("LensStockEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LensCatalogItemId)
            .IsRequired();

        // Prescription powers use decimal(5,2): covers -99.99 to +99.99 (sufficient for clinical range)
        builder.Property(x => x.Sph)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.Cyl)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // Add is nullable (null for single vision lenses)
        builder.Property(x => x.Add)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.MinStockLevel)
            .IsRequired()
            .HasDefaultValue(2);

        // Unique composite index on power combination per catalog item.
        // Prevents duplicate stock entries for the same (Catalog + Sph + Cyl + Add) combination.
        // Add is nullable — the filter accounts for both null and non-null Add values.
        builder.HasIndex(x => new { x.LensCatalogItemId, x.Sph, x.Cyl, x.Add })
            .IsUnique();
    }
}
