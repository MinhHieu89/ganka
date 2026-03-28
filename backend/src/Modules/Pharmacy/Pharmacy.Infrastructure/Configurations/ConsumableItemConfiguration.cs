using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for ConsumableItem.
/// ConsumableItem is the aggregate root for the consumables warehouse (CON-01, CON-02).
/// Supports two tracking modes: ExpiryTracked (batch-level) and SimpleStock (quantity-only).
/// SQL_Latin1_General_Cp1_CI_AI collation on name fields for accent-insensitive search.
/// </summary>
public class ConsumableItemConfiguration : IEntityTypeConfiguration<ConsumableItem>
{
    public void Configure(EntityTypeBuilder<ConsumableItem> builder)
    {
        builder.ToTable("ConsumableItems");

        builder.HasKey(c => c.Id);

        // English name with Vietnamese-collation for accent-insensitive search
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200)
            .UseCollation("SQL_Latin1_General_Cp1_CI_AI");

        // Vietnamese name with Vietnamese-collation for accent-insensitive search
        builder.Property(c => c.NameVi)
            .IsRequired()
            .HasMaxLength(200)
            .UseCollation("SQL_Latin1_General_Cp1_CI_AI");

        builder.Property(c => c.Unit)
            .IsRequired()
            .HasMaxLength(50);

        // ConsumableTrackingMode enum stored as int
        builder.Property(c => c.TrackingMode)
            .IsRequired()
            .HasConversion<int>();

        // CurrentStock is only meaningful for SimpleStock mode; defaults to 0
        builder.Property(c => c.CurrentStock)
            .IsRequired()
            .HasDefaultValue(0);

        // Minimum threshold for low-stock alerts; 0 means no alert configured
        builder.Property(c => c.MinStockLevel)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Search index on Name for catalog lookup
        builder.HasIndex(c => c.Name);
    }
}

/// <summary>
/// EF Core entity configuration for ConsumableBatch.
/// ConsumableBatch is a child entity of ConsumableItem used only when
/// TrackingMode = ExpiryTracked. Mirrors DrugBatch pattern.
/// RowVersion provides optimistic concurrency for concurrent stock deductions.
/// </summary>
public class ConsumableBatchConfiguration : IEntityTypeConfiguration<ConsumableBatch>
{
    public void Configure(EntityTypeBuilder<ConsumableBatch> builder)
    {
        builder.ToTable("ConsumableBatches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ConsumableItemId)
            .IsRequired();

        builder.Property(b => b.BatchNumber)
            .IsRequired()
            .HasMaxLength(100);

        // DateOnly maps natively in EF Core 8+ (no converter needed)
        builder.Property(b => b.ExpiryDate)
            .IsRequired();

        builder.Property(b => b.InitialQuantity)
            .IsRequired();

        builder.Property(b => b.CurrentQuantity)
            .IsRequired();

        // Optimistic concurrency token — prevents concurrent deduction race conditions
        builder.Property(b => b.RowVersion)
            .IsRowVersion();

        // Relationship to parent ConsumableItem
        builder.HasOne<ConsumableItem>()
            .WithMany()
            .HasForeignKey(b => b.ConsumableItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // FEFO index: primary query pattern is ConsumableItemId + ExpiryDate ASC
        builder.HasIndex(b => new { b.ConsumableItemId, b.ExpiryDate })
            .HasDatabaseName("IX_ConsumableBatches_ConsumableItemId_ExpiryDate");
    }
}
