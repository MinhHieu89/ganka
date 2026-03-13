using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optical.Domain.Entities;

namespace Optical.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for GlassesOrder aggregate root and its GlassesOrderItem children.
/// GlassesOrder tracks the full glasses order lifecycle: Ordered -> Processing -> Received -> Ready -> Delivered.
/// Order items are stored in a separate table with cascade delete via backing field navigation.
/// </summary>
public class GlassesOrderConfiguration : IEntityTypeConfiguration<GlassesOrder>
{
    public void Configure(EntityTypeBuilder<GlassesOrder> builder)
    {
        builder.ToTable("GlassesOrders");

        builder.HasKey(x => x.Id);

        // Cross-module FK to Clinical Patient.Id
        builder.Property(x => x.PatientId)
            .IsRequired();

        // Denormalized patient name for display without cross-module join
        builder.Property(x => x.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        // Cross-module FK to Clinical Visit.Id
        builder.Property(x => x.VisitId)
            .IsRequired();

        // Cross-module FK to Clinical OpticalPrescription.Id
        builder.Property(x => x.OpticalPrescriptionId)
            .IsRequired();

        // GlassesOrderStatus stored as int
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        // ProcessingType stored as int
        builder.Property(x => x.ProcessingType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsPaymentConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.EstimatedDeliveryDate)
            .IsRequired(false);

        builder.Property(x => x.DeliveredAt)
            .IsRequired(false);

        builder.Property(x => x.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Cross-module FK to ComboPackage; nullable for custom combos
        builder.Property(x => x.ComboPackageId)
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        // BranchId value object stored as Guid column
        builder.Property(x => x.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Ignore computed properties — not stored as DB columns
        builder.Ignore(x => x.IsUnderWarranty);
        builder.Ignore(x => x.IsOverdue);

        // Backing field navigation for the private _items collection
        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: GlassesOrder -> GlassesOrderItems (cascade delete)
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.GlassesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency token
        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Performance indexes
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.VisitId);
        builder.HasIndex(x => x.Status);
    }
}

/// <summary>
/// EF Core entity configuration for GlassesOrderItem.
/// Child entity of GlassesOrder — always created via GlassesOrder.AddItem().
/// Stores frame and lens line items for each glasses order.
/// </summary>
public class GlassesOrderItemConfiguration : IEntityTypeConfiguration<GlassesOrderItem>
{
    public void Configure(EntityTypeBuilder<GlassesOrderItem> builder)
    {
        builder.ToTable("GlassesOrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GlassesOrderId)
            .IsRequired();

        // Optional FK to Frame catalog item (null for custom/external frames)
        builder.Property(x => x.FrameId)
            .IsRequired(false);

        // Optional FK to LensCatalogItem (null for custom lens orders)
        builder.Property(x => x.LensCatalogItemId)
            .IsRequired(false);

        builder.Property(x => x.ItemDescription)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.ItemDescriptionVi)
            .IsRequired(false)
            .HasMaxLength(300)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Quantity)
            .IsRequired();

        // Ignore computed LineTotal — derived from UnitPrice * Quantity
        builder.Ignore(x => x.LineTotal);
    }
}
