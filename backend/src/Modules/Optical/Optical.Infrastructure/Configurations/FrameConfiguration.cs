using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optical.Domain.Entities;

namespace Optical.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for Frame.
/// Maps the frame catalog aggregate root to the "Frames" table in the optical schema.
/// Critical: Frame.Barcode has a unique filtered index for non-null values to prevent
/// duplicate barcodes from mixed manufacturer/clinic-generated sources (RESEARCH.md pitfall 2).
/// </summary>
public class FrameConfiguration : IEntityTypeConfiguration<Frame>
{
    public void Configure(EntityTypeBuilder<Frame> builder)
    {
        builder.ToTable("Frames");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Color)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LensWidth)
            .IsRequired();

        builder.Property(x => x.BridgeWidth)
            .IsRequired();

        builder.Property(x => x.TempleLength)
            .IsRequired();

        // Enums stored as int
        builder.Property(x => x.Material)
            .IsRequired()
            .HasConversion<int>();

        // Column name "FrameType" avoids potential SQL keyword conflict with "Type"
        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("FrameType");

        builder.Property(x => x.Gender)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.SellingPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.CostPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Barcode is nullable — manufacturer barcodes or clinic-generated EAN-13
        builder.Property(x => x.Barcode)
            .HasMaxLength(13)
            .IsRequired(false);

        // CRITICAL: Unique filtered index on non-null barcodes.
        // Prevents duplicate barcodes from mixed manufacturer and clinic-generated sources.
        // Filter ensures two NULLs don't violate uniqueness (frames without barcodes yet).
        builder.HasIndex(x => x.Barcode)
            .IsUnique()
            .HasFilter("[Barcode] IS NOT NULL");

        builder.Property(x => x.StockQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.MinStockLevel)
            .IsRequired()
            .HasDefaultValue(2);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // BranchId value object stored as Guid column
        builder.Property(x => x.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Ignore computed property — stored in DB as separate columns
        builder.Ignore(x => x.SizeDisplay);

        // Search performance indexes
        builder.HasIndex(x => x.Brand);
        builder.HasIndex(x => x.IsActive);
    }
}
