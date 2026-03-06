using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Aggregate root representing an item in the clinic's consumables warehouse.
/// Consumables are treatment supplies (IPL gel, eye shields, LLLT tips, etc.) tracked
/// separately from the pharmacy drug inventory.
///
/// Supports two tracking modes:
/// - ExpiryTracked: Full batch management with batch numbers, expiry dates, and FEFO dispensing.
///   Stock is computed from the sum of ConsumableBatch.CurrentQuantity.
/// - SimpleStock: Simple quantity counter on the entity itself. No batch records.
///   AddStock()/RemoveStock() mutate CurrentStock directly.
///
/// Auto-deduction from treatment sessions is implemented in Phase 9 (Treatment Protocols)
/// via the Treatment module's session completion workflow.
/// </summary>
public class ConsumableItem : AggregateRoot, IAuditable
{
    /// <summary>English name of the consumable item.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Vietnamese name of the consumable item (with proper diacritics).</summary>
    public string NameVi { get; private set; } = string.Empty;

    /// <summary>
    /// Unit of measure (e.g., "Tube", "Piece", "Pair", "Box").
    /// Used for display and stock reporting.
    /// </summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>
    /// How stock is tracked for this item.
    /// ExpiryTracked = batch-level tracking; SimpleStock = quantity-only.
    /// </summary>
    public ConsumableTrackingMode TrackingMode { get; private set; }

    /// <summary>
    /// Current stock quantity. Only used when TrackingMode is SimpleStock.
    /// For ExpiryTracked items, this value is ignored — stock is computed from ConsumableBatch records.
    /// Guards against negative stock via RemoveStock().
    /// </summary>
    public int CurrentStock { get; private set; }

    /// <summary>
    /// Minimum stock threshold for low-stock alerts.
    /// Alert is raised when stock falls at or below this level.
    /// </summary>
    public int MinStockLevel { get; private set; }

    /// <summary>Whether this item is active and available for use. Soft-delete via deactivation.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Private constructor for EF Core materialization.</summary>
    private ConsumableItem() { }

    /// <summary>
    /// Factory method for creating a new consumable item.
    /// </summary>
    /// <param name="name">English name.</param>
    /// <param name="nameVi">Vietnamese name (with proper diacritics).</param>
    /// <param name="unit">Unit of measure (e.g., "Tube", "Piece").</param>
    /// <param name="trackingMode">How stock is tracked for this item.</param>
    /// <param name="minStockLevel">Minimum stock threshold for low-stock alerts. Defaults to 0.</param>
    /// <param name="branchId">The branch this item belongs to (multi-tenant isolation).</param>
    public static ConsumableItem Create(
        string name,
        string nameVi,
        string unit,
        ConsumableTrackingMode trackingMode,
        int minStockLevel,
        BranchId branchId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Consumable item name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(nameVi))
            throw new ArgumentException("Consumable item Vietnamese name is required.", nameof(nameVi));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit of measure is required.", nameof(unit));

        if (minStockLevel < 0)
            throw new ArgumentException("Minimum stock level cannot be negative.", nameof(minStockLevel));

        var item = new ConsumableItem
        {
            Name = name.Trim(),
            NameVi = nameVi.Trim(),
            Unit = unit.Trim(),
            TrackingMode = trackingMode,
            CurrentStock = 0,
            MinStockLevel = minStockLevel,
            IsActive = true
        };

        item.SetBranchId(branchId);
        return item;
    }

    /// <summary>
    /// Updates the item's metadata. Use when renaming, changing units, or adjusting thresholds.
    /// </summary>
    /// <param name="name">New English name.</param>
    /// <param name="nameVi">New Vietnamese name.</param>
    /// <param name="unit">New unit of measure.</param>
    /// <param name="trackingMode">New tracking mode.</param>
    /// <param name="minStockLevel">New minimum stock threshold.</param>
    public void Update(string name, string nameVi, string unit, ConsumableTrackingMode trackingMode, int minStockLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Consumable item name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(nameVi))
            throw new ArgumentException("Consumable item Vietnamese name is required.", nameof(nameVi));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit of measure is required.", nameof(unit));

        if (minStockLevel < 0)
            throw new ArgumentException("Minimum stock level cannot be negative.", nameof(minStockLevel));

        Name = name.Trim();
        NameVi = nameVi.Trim();
        Unit = unit.Trim();
        TrackingMode = trackingMode;
        MinStockLevel = minStockLevel;
        SetUpdatedAt();
    }

    /// <summary>Deactivates the item so it no longer appears in active inventory.</summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>Re-activates a previously deactivated item.</summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds stock to a SimpleStock-mode item.
    /// Only valid when TrackingMode is SimpleStock. ExpiryTracked items use ConsumableBatch records instead.
    /// </summary>
    /// <param name="qty">Quantity to add (must be positive).</param>
    /// <exception cref="InvalidOperationException">Thrown when called on an ExpiryTracked item.</exception>
    public void AddStock(int qty)
    {
        if (TrackingMode != ConsumableTrackingMode.SimpleStock)
            throw new InvalidOperationException(
                $"Cannot use AddStock on ExpiryTracked item '{Name}'. Use ConsumableBatch records instead.");

        if (qty <= 0)
            throw new ArgumentException("Stock addition quantity must be positive.", nameof(qty));

        CurrentStock += qty;
        SetUpdatedAt();
    }

    /// <summary>
    /// Removes stock from a SimpleStock-mode item.
    /// Guards against negative stock — cannot deduct more than available.
    /// Only valid when TrackingMode is SimpleStock. ExpiryTracked items use ConsumableBatch.Deduct() instead.
    /// </summary>
    /// <param name="qty">Quantity to remove (must be positive and not exceed CurrentStock).</param>
    /// <exception cref="InvalidOperationException">Thrown when called on an ExpiryTracked item or when stock is insufficient.</exception>
    public void RemoveStock(int qty)
    {
        if (TrackingMode != ConsumableTrackingMode.SimpleStock)
            throw new InvalidOperationException(
                $"Cannot use RemoveStock on ExpiryTracked item '{Name}'. Use ConsumableBatch.Deduct() instead.");

        if (qty <= 0)
            throw new ArgumentException("Stock removal quantity must be positive.", nameof(qty));

        if (qty > CurrentStock)
            throw new InvalidOperationException(
                $"Cannot remove {qty} units from '{Name}'. Only {CurrentStock} units available.");

        CurrentStock -= qty;
        SetUpdatedAt();
    }
}
