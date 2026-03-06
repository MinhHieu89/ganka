using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for DrugBatch.
/// Configures the "DrugBatches" table in the pharmacy schema with:
/// - RowVersion for optimistic concurrency (prevents double-dispensing race conditions)
/// - FEFO-optimized composite index on (DrugCatalogItemId, ExpiryDate)
/// - Expiry alert index on ExpiryDate for threshold queries
/// </summary>
public class DrugBatchConfiguration : IEntityTypeConfiguration<DrugBatch>
{
    public void Configure(EntityTypeBuilder<DrugBatch> builder)
    {
        builder.ToTable("DrugBatches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.DrugCatalogItemId)
            .IsRequired();

        builder.Property(b => b.SupplierId)
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

        builder.Property(b => b.PurchasePrice)
            .IsRequired()
            .HasPrecision(18, 2);

        // Nullable FK to stock import transaction
        builder.Property(b => b.StockImportId);

        // Optimistic concurrency token — prevents concurrent dispensing race conditions
        builder.Property(b => b.RowVersion)
            .IsRowVersion();

        // FEFO index: primary query pattern is DrugCatalogItemId + ExpiryDate ASC
        builder.HasIndex(b => new { b.DrugCatalogItemId, b.ExpiryDate })
            .HasDatabaseName("IX_DrugBatches_DrugCatalogItemId_ExpiryDate");

        // Expiry alert index: allows efficient threshold queries (expiry <= today + N days)
        builder.HasIndex(b => b.ExpiryDate)
            .HasDatabaseName("IX_DrugBatches_ExpiryDate");
    }
}
