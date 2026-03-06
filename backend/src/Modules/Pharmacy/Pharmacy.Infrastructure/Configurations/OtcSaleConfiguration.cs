using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for OtcSale and its child OtcSaleLine.
/// OtcSale is the aggregate root for a walk-in over-the-counter sale event.
/// OtcSaleLine is configured as a separate entity type with backing field access
/// for the private _lines collection.
/// </summary>
public class OtcSaleConfiguration : IEntityTypeConfiguration<OtcSale>
{
    public void Configure(EntityTypeBuilder<OtcSale> builder)
    {
        builder.ToTable("OtcSales");

        builder.HasKey(s => s.Id);

        // Optional FK to Patient record — null for fully anonymous sales
        builder.Property(s => s.PatientId);

        // Optional customer name for anonymous walk-in customers
        builder.Property(s => s.CustomerName)
            .HasMaxLength(200);

        // Cross-module FK to Auth User.Id (the staff member processing the sale)
        builder.Property(s => s.SoldById)
            .IsRequired();

        builder.Property(s => s.SoldAt)
            .IsRequired();

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Backing field access for the Lines private collection
        builder.Navigation(s => s.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: OtcSale -> OtcSaleLines (cascade delete)
        builder.HasMany(s => s.Lines)
            .WithOne()
            .HasForeignKey(l => l.OtcSaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core entity configuration for OtcSaleLine.
/// OtcSaleLine is a child of OtcSale. One line per drug item in the sale.
/// Always created via OtcSale.AddLine() — never directly instantiated.
/// </summary>
public class OtcSaleLineConfiguration : IEntityTypeConfiguration<OtcSaleLine>
{
    public void Configure(EntityTypeBuilder<OtcSaleLine> builder)
    {
        builder.ToTable("OtcSaleLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.OtcSaleId)
            .IsRequired();

        // Cross-module FK to Pharmacy DrugCatalogItem.Id
        builder.Property(l => l.DrugCatalogItemId)
            .IsRequired();

        // Denormalized drug name for audit without joins
        builder.Property(l => l.DrugName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Quantity)
            .IsRequired();

        // Selling price snapshot at time of sale (immutable for audit purposes)
        builder.Property(l => l.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        // Backing field access for the BatchDeductions private collection
        builder.Navigation(l => l.BatchDeductions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: OtcSaleLine -> BatchDeductions for FEFO multi-batch deduction
        // BatchDeduction has nullable OtcSaleLineId FK (shared with DispensingLine)
        // Cascade handled via the shared BatchDeductionConfiguration
        builder.HasMany(l => l.BatchDeductions)
            .WithOne()
            .HasForeignKey(bd => bd.OtcSaleLineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
