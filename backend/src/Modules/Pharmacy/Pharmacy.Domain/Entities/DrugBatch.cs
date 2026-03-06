using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Represents a physical batch of a drug received from a supplier.
/// Supports FEFO (First Expiry, First Out) dispensing via ExpiryDate.
/// Stock is deducted at the batch level during dispensing.
/// RowVersion enables optimistic concurrency during concurrent dispensing operations.
/// </summary>
public class DrugBatch : Entity
{
    /// <summary>Foreign key to the DrugCatalogItem this batch belongs to.</summary>
    public Guid DrugCatalogItemId { get; private set; }

    /// <summary>Foreign key to the Supplier who provided this batch.</summary>
    public Guid SupplierId { get; private set; }

    /// <summary>Manufacturer batch number for traceability (e.g., "BN2024001").</summary>
    public string BatchNumber { get; private set; } = string.Empty;

    /// <summary>Expiry date of this batch. Used for FEFO ordering and expiry alerts.</summary>
    public DateOnly ExpiryDate { get; private set; }

    /// <summary>The quantity received when this batch was imported.</summary>
    public int InitialQuantity { get; private set; }

    /// <summary>The current remaining quantity after dispensing deductions.</summary>
    public int CurrentQuantity { get; private set; }

    /// <summary>Purchase price per unit for this batch (VND). Used for cost-of-goods analysis.</summary>
    public decimal PurchasePrice { get; private set; }

    /// <summary>Optional reference to the stock import transaction that created this batch.</summary>
    public Guid? StockImportId { get; private set; }

    /// <summary>Optimistic concurrency token to prevent double-dispensing race conditions.</summary>
    public byte[] RowVersion { get; private set; } = [];

    /// <summary>Private constructor for EF Core materialization.</summary>
    private DrugBatch() { }

    /// <summary>
    /// Factory method for creating a new drug batch.
    /// </summary>
    /// <param name="drugCatalogItemId">The drug this batch belongs to.</param>
    /// <param name="supplierId">The supplier who provided this batch.</param>
    /// <param name="batchNumber">Manufacturer batch number for traceability.</param>
    /// <param name="expiryDate">Expiry date of this batch.</param>
    /// <param name="quantity">Quantity received (must be positive).</param>
    /// <param name="purchasePrice">Purchase price per unit in VND (must be non-negative).</param>
    /// <param name="stockImportId">Optional reference to the stock import transaction.</param>
    public static DrugBatch Create(
        Guid drugCatalogItemId,
        Guid supplierId,
        string batchNumber,
        DateOnly expiryDate,
        int quantity,
        decimal purchasePrice,
        Guid? stockImportId = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Batch quantity must be positive.", nameof(quantity));

        if (purchasePrice < 0)
            throw new ArgumentException("Purchase price cannot be negative.", nameof(purchasePrice));

        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));

        return new DrugBatch
        {
            DrugCatalogItemId = drugCatalogItemId,
            SupplierId = supplierId,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate,
            InitialQuantity = quantity,
            CurrentQuantity = quantity,
            PurchasePrice = purchasePrice,
            StockImportId = stockImportId
        };
    }

    /// <summary>
    /// Deducts the specified quantity from the current batch stock.
    /// Used during drug dispensing (FEFO deduction).
    /// </summary>
    /// <param name="qty">Quantity to deduct (must be positive and not exceed CurrentQuantity).</param>
    /// <exception cref="InvalidOperationException">Thrown when qty exceeds available stock.</exception>
    public void Deduct(int qty)
    {
        if (qty <= 0)
            throw new ArgumentException("Deduction quantity must be positive.", nameof(qty));

        if (qty > CurrentQuantity)
            throw new InvalidOperationException(
                $"Cannot deduct {qty} units from batch '{BatchNumber}'. Only {CurrentQuantity} units available.");

        CurrentQuantity -= qty;
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds stock to the current batch for manual adjustments or correction entries.
    /// </summary>
    /// <param name="qty">Quantity to add (must be positive).</param>
    public void AddStock(int qty)
    {
        if (qty <= 0)
            throw new ArgumentException("Stock addition quantity must be positive.", nameof(qty));

        CurrentQuantity += qty;
        SetUpdatedAt();
    }

    /// <summary>
    /// Returns true if the batch has passed its expiry date (relative to today UTC).
    /// </summary>
    public bool IsExpired => ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Returns true if the batch will expire within the specified number of days.
    /// Does not return true for already-expired batches.
    /// </summary>
    /// <param name="daysThreshold">Number of days from today to check against.</param>
    public bool IsNearExpiry(int daysThreshold) =>
        !IsExpired && ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysThreshold);
}
