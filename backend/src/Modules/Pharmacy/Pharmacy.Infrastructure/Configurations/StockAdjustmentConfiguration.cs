using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for StockAdjustment.
/// StockAdjustment records manual inventory corrections for either a DrugBatch (pharmacy)
/// or a ConsumableBatch (consumables warehouse). Exactly one of the two nullable FKs
/// will be set — the other will be null. This is enforced at the domain level.
///
/// QuantityChange is signed: positive adds stock, negative removes stock.
/// All adjustments are immutable audit records — no update operations.
/// </summary>
public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");

        builder.HasKey(a => a.Id);

        // Nullable FK to DrugBatch — set for pharmacy batch adjustments
        builder.Property(a => a.DrugBatchId);

        // Nullable FK to ConsumableBatch — set for consumables warehouse adjustments
        builder.Property(a => a.ConsumableBatchId);

        // Signed quantity change: positive = addition, negative = removal
        builder.Property(a => a.QuantityChange)
            .IsRequired();

        // StockAdjustmentReason enum stored as int
        builder.Property(a => a.Reason)
            .IsRequired()
            .HasConversion<int>();

        // Optional detail notes about this adjustment
        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        // Cross-module FK to Auth User.Id (pharmacist or warehouse staff)
        builder.Property(a => a.AdjustedById)
            .IsRequired();

        builder.Property(a => a.AdjustedAt)
            .IsRequired();
    }
}
