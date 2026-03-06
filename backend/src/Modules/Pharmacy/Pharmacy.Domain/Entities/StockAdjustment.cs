using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Records a manual stock quantity correction for a drug batch or consumable batch.
/// Used for write-offs, damage, expiry removal, and inventory count corrections.
///
/// Exactly one of DrugBatchId or ConsumableBatchId must be set — this entity is shared
/// between the pharmacy (drug batches) and consumables warehouse (consumable batches).
///
/// QuantityChange is signed: positive adds stock, negative removes stock.
/// QuantityChange must not be zero — a zero-change adjustment has no effect.
/// </summary>
public class StockAdjustment : Entity, IAuditable
{
    /// <summary>
    /// Foreign key to the DrugBatch being adjusted (nullable).
    /// Exactly one of DrugBatchId or ConsumableBatchId will be set.
    /// </summary>
    public Guid? DrugBatchId { get; private set; }

    /// <summary>
    /// Foreign key to the ConsumableBatch being adjusted (nullable).
    /// Exactly one of DrugBatchId or ConsumableBatchId will be set.
    /// </summary>
    public Guid? ConsumableBatchId { get; private set; }

    /// <summary>
    /// Signed quantity change: positive for additions, negative for removals.
    /// Must not be zero — use Deactivate() patterns for permanent discontinuation.
    /// </summary>
    public int QuantityChange { get; private set; }

    /// <summary>Reason category for this adjustment. Used for reporting and audit.</summary>
    public StockAdjustmentReason Reason { get; private set; }

    /// <summary>Optional notes explaining the adjustment details or root cause.</summary>
    public string? Notes { get; private set; }

    /// <summary>Foreign key to the user (pharmacist or warehouse staff) who recorded this adjustment.</summary>
    public Guid AdjustedById { get; private set; }

    /// <summary>UTC timestamp when the adjustment was recorded.</summary>
    public DateTime AdjustedAt { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private StockAdjustment() { }

    /// <summary>
    /// Factory method for creating a new stock adjustment.
    /// Exactly one of <paramref name="drugBatchId"/> or <paramref name="consumableBatchId"/> must be non-null.
    /// </summary>
    /// <param name="drugBatchId">The drug batch to adjust (set for pharmacy adjustments).</param>
    /// <param name="consumableBatchId">The consumable batch to adjust (set for warehouse adjustments).</param>
    /// <param name="quantityChange">Signed quantity change — positive adds, negative removes. Must not be zero.</param>
    /// <param name="reason">Reason category for reporting and audit.</param>
    /// <param name="notes">Optional detail notes about this adjustment.</param>
    /// <param name="adjustedById">The user recording this adjustment.</param>
    public static StockAdjustment Create(
        Guid? drugBatchId,
        Guid? consumableBatchId,
        int quantityChange,
        StockAdjustmentReason reason,
        string? notes,
        Guid adjustedById)
    {
        if (drugBatchId is null && consumableBatchId is null)
            throw new ArgumentException(
                "Exactly one of DrugBatchId or ConsumableBatchId must be set. Both are null.");

        if (drugBatchId is not null && consumableBatchId is not null)
            throw new ArgumentException(
                "Exactly one of DrugBatchId or ConsumableBatchId must be set. Both are non-null.");

        if (quantityChange == 0)
            throw new ArgumentException(
                "QuantityChange must not be zero. Use a positive value to add stock or negative to remove.", nameof(quantityChange));

        return new StockAdjustment
        {
            DrugBatchId = drugBatchId,
            ConsumableBatchId = consumableBatchId,
            QuantityChange = quantityChange,
            Reason = reason,
            Notes = notes,
            AdjustedById = adjustedById,
            AdjustedAt = DateTime.UtcNow
        };
    }
}
