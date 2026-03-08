using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optical.Domain.Entities;

namespace Optical.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for LensOrder.
/// Maps custom lens orders placed with suppliers (Essilor, Hoya, Viet Phap) per patient
/// prescription to the "LensOrders" table in the optical schema.
/// LensOrders are triggered by unusual prescription powers not available in bulk stock.
/// </summary>
public class LensOrderConfiguration : IEntityTypeConfiguration<LensOrder>
{
    public void Configure(EntityTypeBuilder<LensOrder> builder)
    {
        builder.ToTable("LensOrders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LensCatalogItemId)
            .IsRequired();

        // Cross-module reference to Pharmacy.Supplier — stored as plain Guid FK
        builder.Property(x => x.SupplierId)
            .IsRequired();

        builder.Property(x => x.GlassesOrderId)
            .IsRequired();

        builder.Property(x => x.PatientId)
            .IsRequired();

        // Prescription parameters: decimal(5,2) covers clinical ranges (e.g., -20.00 to +20.00)
        builder.Property(x => x.Sph)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.Cyl)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // Add is nullable — only set for bifocal/progressive lens prescriptions
        builder.Property(x => x.Add)
            .HasColumnType("decimal(5,2)");

        // Axis is nullable — only set when cylinder power > 0 (astigmatism correction)
        builder.Property(x => x.Axis)
            .HasColumnType("decimal(5,2)");

        // RequestedCoatings: [Flags] enum stored as combined int value
        builder.Property(x => x.RequestedCoatings)
            .IsRequired()
            .HasConversion<int>();

        // Status stored as string: "Ordered", "Received", "Cancelled"
        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.ReceivedAt)
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        // Index on GlassesOrderId for efficient lookup of orders per glasses order
        builder.HasIndex(x => x.GlassesOrderId);

        // Index on PatientId for patient history queries
        builder.HasIndex(x => x.PatientId);

        // Index on Status for filtering by order state (pending, received, cancelled)
        builder.HasIndex(x => x.Status);
    }
}
