using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Represents a single drug line within a StockImport transaction.
/// One line per drug batch received: captures drug identity, batch details, quantity, and cost.
/// Child entity of StockImport — created exclusively via StockImport.AddLine().
/// </summary>
public class StockImportLine : Entity
{
    /// <summary>Foreign key to the parent StockImport transaction.</summary>
    public Guid StockImportId { get; private set; }

    /// <summary>Foreign key to the DrugCatalogItem this batch belongs to.</summary>
    public Guid DrugCatalogItemId { get; private set; }

    /// <summary>
    /// Denormalized drug name for display without a join.
    /// Snapshot at import time in case the drug catalog entry changes later.
    /// </summary>
    public string DrugName { get; private set; } = string.Empty;

    /// <summary>Manufacturer batch number for regulatory traceability (e.g., "BN2024001").</summary>
    public string BatchNumber { get; private set; } = string.Empty;

    /// <summary>Expiry date of this batch. Used for FEFO ordering and expiry alerts.</summary>
    public DateOnly ExpiryDate { get; private set; }

    /// <summary>Quantity received in this line (must be positive).</summary>
    public int Quantity { get; private set; }

    /// <summary>Purchase price per unit for this batch in VND. Used for cost-of-goods analysis.</summary>
    public decimal PurchasePrice { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private StockImportLine() { }

    /// <summary>
    /// Factory method for creating a new stock import line.
    /// Called exclusively by StockImport.AddLine() — do not create directly.
    /// </summary>
    /// <param name="stockImportId">The parent StockImport this line belongs to.</param>
    /// <param name="drugCatalogItemId">The drug catalog entry for the drug received.</param>
    /// <param name="drugName">Denormalized drug name (snapshot at import time).</param>
    /// <param name="batchNumber">Manufacturer batch number for traceability.</param>
    /// <param name="expiryDate">Expiry date of this batch (must be in the future).</param>
    /// <param name="quantity">Quantity received (must be positive).</param>
    /// <param name="purchasePrice">Purchase price per unit in VND (must be non-negative).</param>
    public static StockImportLine Create(
        Guid stockImportId,
        Guid drugCatalogItemId,
        string drugName,
        string batchNumber,
        DateOnly expiryDate,
        int quantity,
        decimal purchasePrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        if (expiryDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Expiry date must be in the future.", nameof(expiryDate));

        if (purchasePrice < 0)
            throw new ArgumentException("Purchase price cannot be negative.", nameof(purchasePrice));

        if (string.IsNullOrWhiteSpace(drugName))
            throw new ArgumentException("Drug name is required.", nameof(drugName));

        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));

        return new StockImportLine
        {
            StockImportId = stockImportId,
            DrugCatalogItemId = drugCatalogItemId,
            DrugName = drugName,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate,
            Quantity = quantity,
            PurchasePrice = purchasePrice
        };
    }
}
