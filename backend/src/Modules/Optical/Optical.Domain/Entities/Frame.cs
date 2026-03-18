using Optical.Domain.Enums;
using Optical.Domain.Events;
using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// Aggregate root for an eyeglass frame catalog item.
/// Captures all standard optical frame attributes: brand, model, color, size (lens width/bridge/temple),
/// material, frame type, gender, pricing, EAN-13 barcode, and stock quantity.
/// </summary>
public class Frame : AggregateRoot, IAuditable
{
    /// <summary>Brand name of the frame (e.g., "Ray-Ban", "Oakley")</summary>
    public string Brand { get; private set; } = string.Empty;

    /// <summary>Model identifier within the brand (e.g., "RB3025", "OX8156")</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>Color of the frame (e.g., "Matte Black", "Gold")</summary>
    public string Color { get; private set; } = string.Empty;

    /// <summary>Lens width in millimeters (first component of optical size notation, e.g., 52 in 52-18-140)</summary>
    public int LensWidth { get; private set; }

    /// <summary>Bridge width in millimeters (second component of optical size notation, e.g., 18 in 52-18-140)</summary>
    public int BridgeWidth { get; private set; }

    /// <summary>Temple length in millimeters (third component of optical size notation, e.g., 140 in 52-18-140)</summary>
    public int TempleLength { get; private set; }

    /// <summary>Frame material (Metal, Plastic, Titanium)</summary>
    public FrameMaterial Material { get; private set; }

    /// <summary>Frame structure type (FullRim, SemiRimless, Rimless)</summary>
    public FrameType Type { get; private set; }

    /// <summary>Target gender category (Male, Female, Unisex)</summary>
    public FrameGender Gender { get; private set; }

    /// <summary>Retail selling price in VND</summary>
    public decimal SellingPrice { get; private set; }

    /// <summary>Purchase cost price in VND</summary>
    public decimal CostPrice { get; private set; }

    /// <summary>
    /// EAN-13 barcode (13-digit string). Nullable — either scanned from manufacturer label
    /// or auto-generated using clinic prefix via <see cref="Ean13Generator"/>.
    /// </summary>
    public string? Barcode { get; private set; }

    /// <summary>Current on-hand stock quantity for this SKU</summary>
    public int StockQuantity { get; private set; }

    /// <summary>Threshold below which a <see cref="LowStockAlertEvent"/> is raised. Defaults to 2.</summary>
    public int MinStockLevel { get; private set; } = 2;

    /// <summary>Whether this frame is active in the catalog. False = soft-deleted.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Computed display of frame size in standard optical notation: LensWidth-BridgeWidth-TempleLength.
    /// </summary>
    public string SizeDisplay => $"{LensWidth}-{BridgeWidth}-{TempleLength}";

    /// <summary>Private parameterless constructor for EF Core materialization.</summary>
    private Frame() { }

    /// <summary>
    /// Factory method for creating a new frame catalog entry.
    /// Sets StockQuantity to 0 and IsActive to true. Raises no events on creation.
    /// </summary>
    /// <param name="brand">Frame brand name (required).</param>
    /// <param name="model">Frame model identifier (required).</param>
    /// <param name="color">Frame color description (required).</param>
    /// <param name="lensWidth">Lens width in mm (optical size first component).</param>
    /// <param name="bridgeWidth">Bridge width in mm (optical size second component).</param>
    /// <param name="templeLength">Temple length in mm (optical size third component).</param>
    /// <param name="material">Frame material type.</param>
    /// <param name="type">Frame structure type.</param>
    /// <param name="gender">Target gender category.</param>
    /// <param name="sellingPrice">Retail selling price in VND.</param>
    /// <param name="costPrice">Purchase cost price in VND.</param>
    /// <param name="barcode">Optional EAN-13 barcode. Null if manufacturer barcode not available yet.</param>
    /// <param name="branchId">Branch this frame belongs to (multi-tenant isolation).</param>
    public static Frame Create(
        string brand,
        string model,
        string color,
        int lensWidth,
        int bridgeWidth,
        int templeLength,
        FrameMaterial material,
        FrameType type,
        FrameGender gender,
        decimal sellingPrice,
        decimal costPrice,
        string? barcode,
        BranchId branchId,
        int stockQuantity = 0)
    {
        var frame = new Frame
        {
            Brand = brand,
            Model = model,
            Color = color,
            LensWidth = lensWidth,
            BridgeWidth = bridgeWidth,
            TempleLength = templeLength,
            Material = material,
            Type = type,
            Gender = gender,
            SellingPrice = sellingPrice,
            CostPrice = costPrice,
            Barcode = barcode,
            StockQuantity = stockQuantity,
            MinStockLevel = 2,
            IsActive = true
        };

        frame.SetBranchId(branchId);
        return frame;
    }

    /// <summary>
    /// Updates all editable attributes of the frame including stock quantity.
    /// </summary>
    public void Update(
        string brand,
        string model,
        string color,
        int lensWidth,
        int bridgeWidth,
        int templeLength,
        FrameMaterial material,
        FrameType type,
        FrameGender gender,
        decimal sellingPrice,
        decimal costPrice,
        string? barcode,
        int? stockQuantity = null)
    {
        Brand = brand;
        Model = model;
        Color = color;
        LensWidth = lensWidth;
        BridgeWidth = bridgeWidth;
        TempleLength = templeLength;
        Material = material;
        Type = type;
        Gender = gender;
        SellingPrice = sellingPrice;
        CostPrice = costPrice;
        Barcode = barcode;
        if (stockQuantity.HasValue)
            StockQuantity = stockQuantity.Value;

        SetUpdatedAt();
    }

    /// <summary>
    /// Adjusts the stock quantity by the given delta (positive = restock, negative = deduction).
    /// Raises <see cref="LowStockAlertEvent"/> when stock falls at or below <see cref="MinStockLevel"/>.
    /// </summary>
    /// <param name="quantityChange">Positive value to increase stock, negative to decrease.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the adjustment would result in a negative stock quantity.
    /// </exception>
    public void AdjustStock(int quantityChange)
    {
        var newQuantity = StockQuantity + quantityChange;

        if (newQuantity < 0)
        {
            throw new InvalidOperationException(
                $"Cannot reduce stock below zero. Current: {StockQuantity}, Change: {quantityChange}.");
        }

        StockQuantity = newQuantity;
        SetUpdatedAt();

        if (quantityChange < 0 && StockQuantity <= MinStockLevel)
        {
            AddDomainEvent(new LowStockAlertEvent(
                EntityId: Id,
                EntityType: "Frame",
                Name: $"{Brand} {Model} ({Color}, {SizeDisplay})",
                CurrentStock: StockQuantity,
                MinStockLevel: MinStockLevel));
        }
    }

    /// <summary>
    /// Soft-deactivates the frame, hiding it from catalog selection.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Re-activates a previously deactivated frame.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
