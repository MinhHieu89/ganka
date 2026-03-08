using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// Represents a line item on a glasses order — either a frame or a lens selection.
/// Child entity of <see cref="GlassesOrder"/>; not an aggregate root.
/// </summary>
public class GlassesOrderItem : Entity
{
    /// <summary>Foreign key to the parent GlassesOrder.</summary>
    public Guid GlassesOrderId { get; private set; }

    /// <summary>
    /// Optional FK to a Frame catalog item. Null if this item is a lens-only line.
    /// </summary>
    public Guid? FrameId { get; private set; }

    /// <summary>
    /// Optional FK to a LensCatalogItem. Null if this item is a frame-only line.
    /// </summary>
    public Guid? LensCatalogItemId { get; private set; }

    /// <summary>
    /// Denormalized item description for display and audit purposes.
    /// E.g., "Ray-Ban RB3025 Black 52-18-140" or "Essilor Crizal Single Vision".
    /// </summary>
    public string ItemDescription { get; private set; } = string.Empty;

    /// <summary>Unit price for this line item in VND at the time of order creation.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Quantity ordered (typically 1 for frames, 2 for a lens pair).</summary>
    public int Quantity { get; private set; }

    /// <summary>Total line price: UnitPrice * Quantity in VND.</summary>
    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>Private parameterless constructor for EF Core materialization.</summary>
    private GlassesOrderItem() { }

    /// <summary>
    /// Factory method for creating a glasses order item (frame or lens).
    /// </summary>
    /// <param name="glassesOrderId">ID of the parent order.</param>
    /// <param name="frameId">Optional FK to Frame catalog item.</param>
    /// <param name="lensCatalogItemId">Optional FK to LensCatalogItem.</param>
    /// <param name="itemDescription">Display name for this line item.</param>
    /// <param name="unitPrice">Price per unit in VND.</param>
    /// <param name="quantity">Number of units (typically 1 or 2).</param>
    public static GlassesOrderItem Create(
        Guid glassesOrderId,
        Guid? frameId,
        Guid? lensCatalogItemId,
        string itemDescription,
        decimal unitPrice,
        int quantity)
    {
        if (glassesOrderId == Guid.Empty)
            throw new ArgumentException("GlassesOrderId is required.", nameof(glassesOrderId));

        if (string.IsNullOrWhiteSpace(itemDescription))
            throw new ArgumentException("Item description is required.", nameof(itemDescription));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        return new GlassesOrderItem
        {
            GlassesOrderId = glassesOrderId,
            FrameId = frameId,
            LensCatalogItemId = lensCatalogItemId,
            ItemDescription = itemDescription,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }
}
