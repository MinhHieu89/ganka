using Optical.Domain.Enums;
using Optical.Domain.Events;
using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// Aggregate root representing a lens type in the optical catalog.
/// Supports the hybrid lens model: bulk stock for common powers + custom orders per prescription.
/// Tracks brand, type, material, coating options, and pricing.
/// Suppliers: Essilor, Hoya, Viet Phap (cross-module reference to Pharmacy.Supplier).
/// </summary>
public class LensCatalogItem : AggregateRoot, IAuditable
{
    private readonly List<LensStockEntry> _stockEntries = new();

    /// <summary>Lens manufacturer/brand (e.g., "Essilor", "Hoya", "Viet Phap")</summary>
    public string Brand { get; private set; } = string.Empty;

    /// <summary>Lens product name (e.g., "Essilor Crizal Single Vision")</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Lens type as a string for flexibility.
    /// Common values: "single_vision", "bifocal", "progressive", "reading"
    /// </summary>
    public string LensType { get; private set; } = string.Empty;

    /// <summary>Lens material (CR-39, Polycarbonate, Hi-Index, Trivex)</summary>
    public LensMaterial Material { get; private set; }

    /// <summary>
    /// Available coating options for this lens. [Flags] enum — multiple coatings can be combined.
    /// E.g., AntiReflective | BlueCut
    /// </summary>
    public LensCoating AvailableCoatings { get; private set; }

    /// <summary>Retail selling price per lens pair in VND.</summary>
    public decimal SellingPrice { get; private set; }

    /// <summary>Cost price per lens pair in VND (from supplier).</summary>
    public decimal CostPrice { get; private set; }

    /// <summary>Whether this catalog item is active and visible to optical staff.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Optional preferred supplier ID. Cross-module reference to Pharmacy.Supplier entity.
    /// Null if no preferred supplier is configured.
    /// </summary>
    public Guid? PreferredSupplierId { get; private set; }

    /// <summary>
    /// Stock entries for specific power combinations (SPH/CYL/ADD).
    /// Populated for commonly stocked lens powers; uncommon powers use custom LensOrder.
    /// </summary>
    public IReadOnlyList<LensStockEntry> StockEntries => _stockEntries.AsReadOnly();

    /// <summary>Private parameterless constructor for EF Core materialization.</summary>
    private LensCatalogItem() { }

    /// <summary>
    /// Factory method for creating a new lens catalog item.
    /// </summary>
    /// <param name="brand">Lens brand/manufacturer name.</param>
    /// <param name="name">Full product name for display.</param>
    /// <param name="lensType">Lens type string (e.g., "single_vision", "progressive").</param>
    /// <param name="material">Lens material composition.</param>
    /// <param name="availableCoatings">Bitwise combination of available coating options.</param>
    /// <param name="sellingPrice">Retail price per lens pair in VND.</param>
    /// <param name="costPrice">Supplier cost per lens pair in VND.</param>
    /// <param name="supplierId">Optional preferred supplier cross-module reference.</param>
    /// <param name="branchId">Branch this catalog item belongs to.</param>
    public static LensCatalogItem Create(
        string brand,
        string name,
        string lensType,
        LensMaterial material,
        LensCoating availableCoatings,
        decimal sellingPrice,
        decimal costPrice,
        Guid? supplierId,
        BranchId branchId)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand is required.", nameof(brand));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(lensType))
            throw new ArgumentException("Lens type is required.", nameof(lensType));

        if (sellingPrice < 0)
            throw new ArgumentException("Selling price cannot be negative.", nameof(sellingPrice));

        if (costPrice < 0)
            throw new ArgumentException("Cost price cannot be negative.", nameof(costPrice));

        var item = new LensCatalogItem
        {
            Brand = brand,
            Name = name,
            LensType = lensType,
            Material = material,
            AvailableCoatings = availableCoatings,
            SellingPrice = sellingPrice,
            CostPrice = costPrice,
            PreferredSupplierId = supplierId,
            IsActive = true
        };

        item.SetBranchId(branchId);
        return item;
    }

    /// <summary>
    /// Updates the catalog item's editable properties.
    /// </summary>
    public void Update(
        string brand,
        string name,
        string lensType,
        LensMaterial material,
        LensCoating availableCoatings,
        decimal sellingPrice,
        decimal costPrice,
        Guid? supplierId)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand is required.", nameof(brand));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(lensType))
            throw new ArgumentException("Lens type is required.", nameof(lensType));

        if (sellingPrice < 0)
            throw new ArgumentException("Selling price cannot be negative.", nameof(sellingPrice));

        if (costPrice < 0)
            throw new ArgumentException("Cost price cannot be negative.", nameof(costPrice));

        Brand = brand;
        Name = name;
        LensType = lensType;
        Material = material;
        AvailableCoatings = availableCoatings;
        SellingPrice = sellingPrice;
        CostPrice = costPrice;
        PreferredSupplierId = supplierId;

        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a stock entry for a specific power combination (SPH/CYL/ADD).
    /// Used when stocking common lens powers in bulk.
    /// </summary>
    /// <param name="sph">Spherical power (e.g., -2.00, +1.50).</param>
    /// <param name="cyl">Cylinder power (e.g., -0.75, 0 for sphere-only).</param>
    /// <param name="add">Addition power for bifocal/progressive lenses. Null for single vision.</param>
    /// <param name="quantity">Initial quantity in stock (must be non-negative).</param>
    /// <returns>The newly created LensStockEntry.</returns>
    public LensStockEntry AddStockEntry(decimal sph, decimal cyl, decimal? add, int quantity)
    {
        var entry = LensStockEntry.Create(Id, sph, cyl, add, quantity);
        _stockEntries.Add(entry);
        SetUpdatedAt();
        return entry;
    }

    /// <summary>
    /// Adjusts stock for a specific stock entry by entry ID, and raises a LowStockAlertEvent
    /// on this aggregate if the quantity falls below the minimum stock level after adjustment.
    /// </summary>
    /// <param name="stockEntryId">ID of the LensStockEntry to adjust.</param>
    /// <param name="change">Amount to adjust (positive = add stock, negative = deduct stock).</param>
    /// <exception cref="InvalidOperationException">Thrown if the stock entry is not found or adjustment goes negative.</exception>
    public void AdjustStockEntry(Guid stockEntryId, int change)
    {
        var entry = _stockEntries.FirstOrDefault(e => e.Id == stockEntryId)
            ?? throw new InvalidOperationException($"Stock entry '{stockEntryId}' not found on this catalog item.");

        entry.AdjustStock(change);

        if (entry.IsLowStock)
        {
            AddDomainEvent(new LowStockAlertEvent(
                EntityId: Id,
                EntityType: "Lens",
                Name: $"{Brand} {Name} (SPH={entry.Sph}/CYL={entry.Cyl})",
                CurrentStock: entry.Quantity,
                MinStockLevel: entry.MinStockLevel));
        }

        SetUpdatedAt();
    }

    /// <summary>
    /// Deactivates this lens catalog item, hiding it from optical staff searches.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Re-activates a previously deactivated lens catalog item.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
