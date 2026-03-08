using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// Represents the stock quantity for a specific power combination of a lens catalog item.
/// Supports per-piece quantity tracking for common lens powers stored in bulk.
/// Low-stock alerts fire when quantity drops below MinStockLevel.
/// </summary>
public class LensStockEntry : Entity
{
    /// <summary>Foreign key to the parent LensCatalogItem.</summary>
    public Guid LensCatalogItemId { get; private set; }

    /// <summary>
    /// Spherical power of this stock entry.
    /// Negative values for myopia (e.g., -2.00), positive for hyperopia (e.g., +1.50).
    /// </summary>
    public decimal Sph { get; private set; }

    /// <summary>
    /// Cylinder power for astigmatism correction (e.g., -0.75).
    /// Use 0 for sphere-only lenses with no astigmatism correction.
    /// </summary>
    public decimal Cyl { get; private set; }

    /// <summary>
    /// Addition power for bifocal or progressive lenses (e.g., +2.00).
    /// Null for single vision lenses.
    /// </summary>
    public decimal? Add { get; private set; }

    /// <summary>Current quantity of lenses in stock for this power combination.</summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Minimum stock level threshold. Low-stock alerts fire when Quantity drops below this value.
    /// Defaults to 2 pieces.
    /// </summary>
    public int MinStockLevel { get; private set; } = 2;

    /// <summary>Private parameterless constructor for EF Core materialization.</summary>
    private LensStockEntry() { }

    /// <summary>
    /// Factory method for creating a new lens stock entry for a specific power combination.
    /// </summary>
    /// <param name="lensCatalogItemId">Parent lens catalog item ID.</param>
    /// <param name="sph">Spherical power.</param>
    /// <param name="cyl">Cylinder power (0 for sphere-only).</param>
    /// <param name="add">Addition power for bifocal/progressive. Null for single vision.</param>
    /// <param name="quantity">Initial stock quantity (must be non-negative).</param>
    public static LensStockEntry Create(
        Guid lensCatalogItemId,
        decimal sph,
        decimal cyl,
        decimal? add,
        int quantity)
    {
        if (lensCatalogItemId == Guid.Empty)
            throw new ArgumentException("LensCatalogItemId is required.", nameof(lensCatalogItemId));

        if (quantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative.", nameof(quantity));

        return new LensStockEntry
        {
            LensCatalogItemId = lensCatalogItemId,
            Sph = sph,
            Cyl = cyl,
            Add = add,
            Quantity = quantity,
            MinStockLevel = 2
        };
    }

    /// <summary>
    /// Adjusts the stock quantity by the given change (positive = add stock, negative = deduct stock).
    /// Throws if the resulting quantity would go negative.
    /// Callers should check IsLowStock after adjustment and raise alerts accordingly.
    /// </summary>
    /// <param name="change">Amount to adjust. Positive to receive stock, negative to deduct.</param>
    /// <exception cref="InvalidOperationException">Thrown when deduction would result in negative stock.</exception>
    public void AdjustStock(int change)
    {
        if (Quantity + change < 0)
            throw new InvalidOperationException(
                $"Cannot deduct {Math.Abs(change)} units. Only {Quantity} units available for SPH={Sph}/CYL={Cyl}.");

        Quantity += change;
        SetUpdatedAt();
    }

    /// <summary>
    /// Returns true when current quantity is below the minimum stock level threshold.
    /// </summary>
    public bool IsLowStock => Quantity < MinStockLevel;

    /// <summary>
    /// Updates the minimum stock level for this power combination.
    /// </summary>
    /// <param name="minStockLevel">New minimum stock level (must be non-negative).</param>
    public void UpdateMinStockLevel(int minStockLevel)
    {
        if (minStockLevel < 0)
            throw new ArgumentException("Minimum stock level cannot be negative.", nameof(minStockLevel));

        MinStockLevel = minStockLevel;
        SetUpdatedAt();
    }
}
