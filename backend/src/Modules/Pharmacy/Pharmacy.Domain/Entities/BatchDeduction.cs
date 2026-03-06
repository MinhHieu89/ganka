using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Records the quantity deducted from a specific drug batch during a dispensing or OTC sale event.
/// Links a batch quantity reduction to either a DispensingLine (prescription dispensing) or an
/// OtcSaleLine (walk-in OTC sale). Exactly one parent FK must be non-null.
///
/// Supports multi-batch deduction: a single prescription line may span multiple batches
/// when FEFO allocation pulls from more than one batch to fulfil the required quantity.
/// </summary>
public class BatchDeduction : Entity
{
    /// <summary>
    /// Foreign key to the DispensingLine this deduction belongs to.
    /// Null when this deduction is for an OTC sale line.
    /// </summary>
    public Guid? DispensingLineId { get; private set; }

    /// <summary>
    /// Foreign key to the OtcSaleLine this deduction belongs to.
    /// Null when this deduction is for a dispensing line.
    /// </summary>
    public Guid? OtcSaleLineId { get; private set; }

    /// <summary>Foreign key to the DrugBatch from which stock was deducted.</summary>
    public Guid DrugBatchId { get; private set; }

    /// <summary>
    /// Batch number denormalized from DrugBatch for audit reporting without requiring a join.
    /// </summary>
    public string BatchNumber { get; private set; } = string.Empty;

    /// <summary>Number of units deducted from this batch. Must be positive.</summary>
    public int Quantity { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private BatchDeduction() { }

    /// <summary>
    /// Factory method for creating a batch deduction linked to a dispensing line.
    /// Used during prescription dispensing with FEFO batch selection.
    /// </summary>
    /// <param name="dispensingLineId">The dispensing line this deduction belongs to.</param>
    /// <param name="drugBatchId">The batch from which stock is deducted.</param>
    /// <param name="batchNumber">Batch number (denormalized from DrugBatch).</param>
    /// <param name="quantity">Quantity deducted (must be positive).</param>
    public static BatchDeduction CreateForDispensing(
        Guid dispensingLineId,
        Guid drugBatchId,
        string batchNumber,
        int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Deduction quantity must be positive.", nameof(quantity));

        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));

        return new BatchDeduction
        {
            DispensingLineId = dispensingLineId,
            OtcSaleLineId = null,
            DrugBatchId = drugBatchId,
            BatchNumber = batchNumber,
            Quantity = quantity
        };
    }

    /// <summary>
    /// Factory method for creating a batch deduction linked to an OTC sale line.
    /// Used during walk-in OTC sales with FEFO batch selection.
    /// </summary>
    /// <param name="otcSaleLineId">The OTC sale line this deduction belongs to.</param>
    /// <param name="drugBatchId">The batch from which stock is deducted.</param>
    /// <param name="batchNumber">Batch number (denormalized from DrugBatch).</param>
    /// <param name="quantity">Quantity deducted (must be positive).</param>
    public static BatchDeduction CreateForOtcSale(
        Guid otcSaleLineId,
        Guid drugBatchId,
        string batchNumber,
        int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Deduction quantity must be positive.", nameof(quantity));

        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));

        return new BatchDeduction
        {
            DispensingLineId = null,
            OtcSaleLineId = otcSaleLineId,
            DrugBatchId = drugBatchId,
            BatchNumber = batchNumber,
            Quantity = quantity
        };
    }
}
