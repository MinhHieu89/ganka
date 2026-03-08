using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optical.Domain.Entities;

namespace Optical.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for ComboPackage.
/// ComboPackage represents a preset named frame + lens bundle at a discounted combo price.
/// Admin creates combo packages; optical staff selects them at order creation time.
/// </summary>
public class ComboPackageConfiguration : IEntityTypeConfiguration<ComboPackage>
{
    public void Configure(EntityTypeBuilder<ComboPackage> builder)
    {
        builder.ToTable("ComboPackages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Optional FK to a specific Frame in the catalog
        builder.Property(x => x.FrameId)
            .IsRequired(false);

        // Optional FK to a specific LensCatalogItem
        builder.Property(x => x.LensCatalogItemId)
            .IsRequired(false);

        // The bundled price for the full combo in VND
        builder.Property(x => x.ComboPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Optional sum of individual prices for savings display
        builder.Property(x => x.OriginalTotalPrice)
            .IsRequired(false)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // BranchId value object stored as Guid column
        builder.Property(x => x.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Ignore computed property — derived from OriginalTotalPrice - ComboPrice
        builder.Ignore(x => x.SavingsAmount);

        // Performance indexes
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Name);
    }
}
