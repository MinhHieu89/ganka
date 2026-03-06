using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Represents one prescription drug line within a dispensing event.
/// Each DispensingLine corresponds to a single PrescriptionItem from the Clinical module.
/// Status is all-or-nothing per line: Dispensed or Skipped.
///
/// When status is Dispensed, BatchDeductions record exactly which batches (and quantities)
/// were deducted to fulfil this line. A single line may span multiple batches via FEFO.
/// </summary>
public class DispensingLine : Entity
{
    /// <summary>Foreign key back to the parent DispensingRecord aggregate.</summary>
    public Guid DispensingRecordId { get; private set; }

    /// <summary>
    /// Foreign key to the Clinical PrescriptionItem.Id this line fulfils.
    /// Denormalized reference -- Clinical module is the source of truth.
    /// </summary>
    public Guid PrescriptionItemId { get; private set; }

    /// <summary>Foreign key to the Pharmacy DrugCatalogItem being dispensed.</summary>
    public Guid DrugCatalogItemId { get; private set; }

    /// <summary>Drug name denormalized from DrugCatalogItem for audit records without cross-module joins.</summary>
    public string DrugName { get; private set; } = string.Empty;

    /// <summary>Quantity prescribed on this line.</summary>
    public int Quantity { get; private set; }

    /// <summary>Whether this line was dispensed or intentionally skipped.</summary>
    public DispensingStatus Status { get; private set; }

    private readonly List<BatchDeduction> _batchDeductions = [];

    /// <summary>
    /// Batch deductions linked to this line. Only populated when Status is Dispensed.
    /// Multiple deductions occur when FEFO allocation spans more than one batch.
    /// </summary>
    public IReadOnlyCollection<BatchDeduction> BatchDeductions => _batchDeductions.AsReadOnly();

    /// <summary>Private constructor for EF Core materialization.</summary>
    private DispensingLine() { }

    /// <summary>
    /// Internal factory used by DispensingRecord.AddLine().
    /// Not public — DispensingLine is always created through its aggregate root.
    /// </summary>
    internal static DispensingLine Create(
        Guid dispensingRecordId,
        Guid prescriptionItemId,
        Guid drugCatalogItemId,
        string drugName,
        int quantity,
        DispensingStatus status)
    {
        if (quantity <= 0)
            throw new ArgumentException("Dispensing line quantity must be positive.", nameof(quantity));

        if (string.IsNullOrWhiteSpace(drugName))
            throw new ArgumentException("Drug name is required.", nameof(drugName));

        return new DispensingLine
        {
            DispensingRecordId = dispensingRecordId,
            PrescriptionItemId = prescriptionItemId,
            DrugCatalogItemId = drugCatalogItemId,
            DrugName = drugName,
            Quantity = quantity,
            Status = status
        };
    }

    /// <summary>
    /// Adds a batch deduction record to this line.
    /// Called once per batch when FEFO allocation spans multiple batches.
    /// </summary>
    /// <param name="drugBatchId">The batch from which stock was deducted.</param>
    /// <param name="batchNumber">Batch number (denormalized for audit).</param>
    /// <param name="quantity">Quantity deducted from this batch.</param>
    public void AddBatchDeduction(Guid drugBatchId, string batchNumber, int quantity)
    {
        var deduction = BatchDeduction.CreateForDispensing(Id, drugBatchId, batchNumber, quantity);
        _batchDeductions.Add(deduction);
    }
}
