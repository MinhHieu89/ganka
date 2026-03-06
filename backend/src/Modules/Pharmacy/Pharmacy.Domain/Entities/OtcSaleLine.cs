using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Represents one drug item line within an OTC sale.
/// Each OtcSaleLine corresponds to a single drug being sold to the customer.
///
/// UnitPrice is a snapshot of the selling price at the time of sale — price changes
/// on the DrugCatalogItem after the sale do not affect historical records.
///
/// BatchDeductions record exactly which batches (and quantities) were deducted
/// to fulfil this line. A single line may span multiple batches via FEFO.
/// </summary>
public class OtcSaleLine : Entity
{
    /// <summary>Foreign key back to the parent OtcSale aggregate.</summary>
    public Guid OtcSaleId { get; private set; }

    /// <summary>Foreign key to the Pharmacy DrugCatalogItem being sold.</summary>
    public Guid DrugCatalogItemId { get; private set; }

    /// <summary>Drug name denormalized from DrugCatalogItem for audit records without cross-module joins.</summary>
    public string DrugName { get; private set; } = string.Empty;

    /// <summary>Quantity being sold (must be positive).</summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Selling price per unit at the time of sale (VND).
    /// Snapshot of DrugCatalogItem.SellingPrice at the time of sale — immutable for audit purposes.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    private readonly List<BatchDeduction> _batchDeductions = [];

    /// <summary>
    /// Batch deductions linked to this sale line.
    /// Multiple deductions occur when FEFO allocation spans more than one batch.
    /// </summary>
    public IReadOnlyCollection<BatchDeduction> BatchDeductions => _batchDeductions.AsReadOnly();

    /// <summary>Private constructor for EF Core materialization.</summary>
    private OtcSaleLine() { }

    /// <summary>
    /// Internal factory used by OtcSale.AddLine().
    /// Not public — OtcSaleLine is always created through its aggregate root.
    /// </summary>
    internal static OtcSaleLine Create(
        Guid otcSaleId,
        Guid drugCatalogItemId,
        string drugName,
        int quantity,
        decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("OTC sale line quantity must be positive.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        if (string.IsNullOrWhiteSpace(drugName))
            throw new ArgumentException("Drug name is required.", nameof(drugName));

        return new OtcSaleLine
        {
            OtcSaleId = otcSaleId,
            DrugCatalogItemId = drugCatalogItemId,
            DrugName = drugName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    /// <summary>
    /// Adds a batch deduction record to this sale line.
    /// Called once per batch when FEFO allocation spans multiple batches.
    /// Uses BatchDeduction.CreateForOtcSale factory to set the correct parent FK.
    /// </summary>
    /// <param name="drugBatchId">The DrugBatch from which stock was deducted.</param>
    /// <param name="batchNumber">Batch number denormalized for audit records.</param>
    /// <param name="quantity">Quantity deducted from this batch.</param>
    public void AddBatchDeduction(Guid drugBatchId, string batchNumber, int quantity)
    {
        var deduction = BatchDeduction.CreateForOtcSale(Id, drugBatchId, batchNumber, quantity);
        _batchDeductions.Add(deduction);
    }
}
