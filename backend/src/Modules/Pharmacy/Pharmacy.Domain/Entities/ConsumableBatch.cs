using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Represents a physical batch of a consumable item received into the consumables warehouse.
/// Only used when the parent ConsumableItem has TrackingMode = ExpiryTracked.
///
/// Mirrors the DrugBatch pattern: supports FEFO (First Expiry, First Out) via ExpiryDate,
/// deducts stock at batch level, and uses RowVersion for optimistic concurrency.
///
/// For SimpleStock items, stock is tracked directly on ConsumableItem.CurrentStock.
/// </summary>
public class ConsumableBatch : Entity
{
    /// <summary>Foreign key to the ConsumableItem this batch belongs to.</summary>
    public Guid ConsumableItemId { get; private set; }

    /// <summary>Manufacturer or supplier batch number for traceability (e.g., "IPL-2024-001").</summary>
    public string BatchNumber { get; private set; } = string.Empty;

    /// <summary>Expiry date of this batch. Used for FEFO ordering and expiry alerts.</summary>
    public DateOnly ExpiryDate { get; private set; }

    /// <summary>The quantity received when this batch was imported.</summary>
    public int InitialQuantity { get; private set; }

    /// <summary>The current remaining quantity after deductions.</summary>
    public int CurrentQuantity { get; private set; }

    /// <summary>Optimistic concurrency token to prevent double-deduction race conditions.</summary>
    public byte[] RowVersion { get; private set; } = [];

    /// <summary>Private constructor for EF Core materialization.</summary>
    private ConsumableBatch() { }

    /// <summary>
    /// Factory method for creating a new consumable batch.
    /// Only call this for ExpiryTracked consumable items.
    /// </summary>
    /// <param name="consumableItemId">The consumable item this batch belongs to.</param>
    /// <param name="batchNumber">Manufacturer/supplier batch number for traceability.</param>
    /// <param name="expiryDate">Expiry date of this batch.</param>
    /// <param name="quantity">Quantity received (must be positive).</param>
    public static ConsumableBatch Create(
        Guid consumableItemId,
        string batchNumber,
        DateOnly expiryDate,
        int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Batch quantity must be positive.", nameof(quantity));

        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));

        return new ConsumableBatch
        {
            ConsumableItemId = consumableItemId,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate,
            InitialQuantity = quantity,
            CurrentQuantity = quantity
        };
    }

    /// <summary>
    /// Adds stock to the current batch for manual adjustments or correction entries.
    /// Mirrors DrugBatch.AddStock for symmetry.
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
    /// Deducts the specified quantity from this batch's current stock.
    /// Used during FEFO-ordered consumable deduction -- manually via stock management,
    /// or automatically from treatment sessions (Phase 9 Treatment Protocols).
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
