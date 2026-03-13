using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// AggregateRoot for a preset named combo package combining a specific frame and lens type
/// at a bundled price — typically discounted compared to purchasing each item separately.
/// Admin creates combo packages; optical staff selects them at order creation time.
/// The savings amount (OriginalTotalPrice - ComboPrice) is displayed to the customer.
/// </summary>
public class ComboPackage : AggregateRoot, IAuditable
{
    /// <summary>
    /// Display name for the package (e.g., "Rayban Classic + Essilor Crizal SV").
    /// Used as the primary identifier in combo selection dropdowns.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional description providing additional details about the combo contents.</summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Foreign key to a specific Frame in the catalog.
    /// Null if the combo is lens-only or allows any compatible frame.
    /// </summary>
    public Guid? FrameId { get; private set; }

    /// <summary>
    /// Foreign key to a specific LensCatalogItem.
    /// Null if the combo is frame-only or allows any compatible lens.
    /// </summary>
    public Guid? LensCatalogItemId { get; private set; }

    /// <summary>
    /// The bundled price for the full combo in VND.
    /// This is what the patient pays — typically less than the sum of individual prices.
    /// </summary>
    public decimal ComboPrice { get; private set; }

    /// <summary>
    /// Optional sum of individual item prices (before the combo discount).
    /// Displayed alongside ComboPrice to show the patient their savings.
    /// Null when individual prices are not available or not relevant.
    /// </summary>
    public decimal? OriginalTotalPrice { get; private set; }

    /// <summary>Whether this combo package is currently available for selection at order time.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Private constructor for EF Core materialization.</summary>
    private ComboPackage() { }

    /// <summary>
    /// Factory method for creating a new combo package.
    /// </summary>
    /// <param name="name">Required display name (e.g., "Rayban Classic + Essilor Crizal SV").</param>
    /// <param name="description">Optional description of the package contents.</param>
    /// <param name="frameId">Optional FK to a specific Frame; null for frame-flexible combos.</param>
    /// <param name="lensCatalogItemId">Optional FK to a specific LensCatalogItem; null for lens-flexible combos.</param>
    /// <param name="comboPrice">The bundled price in VND (must be greater than zero).</param>
    /// <param name="originalTotalPrice">Optional sum of individual prices for savings display.</param>
    /// <param name="branchId">The branch this combo package belongs to.</param>
    public static ComboPackage Create(
        string name,
        string? description,
        Guid? frameId,
        Guid? lensCatalogItemId,
        decimal comboPrice,
        decimal? originalTotalPrice,
        BranchId branchId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Combo package name is required.", nameof(name));

        if (comboPrice <= 0)
            throw new ArgumentException("Combo price must be greater than zero.", nameof(comboPrice));

        if (originalTotalPrice.HasValue && originalTotalPrice.Value <= 0)
            throw new ArgumentException("Original total price must be greater than zero when provided.", nameof(originalTotalPrice));

        var combo = new ComboPackage
        {
            Name = name,
            Description = description,
            FrameId = frameId,
            LensCatalogItemId = lensCatalogItemId,
            ComboPrice = comboPrice,
            OriginalTotalPrice = originalTotalPrice,
            IsActive = true
        };

        combo.SetBranchId(branchId);
        return combo;
    }

    /// <summary>
    /// Updates the editable fields of the combo package.
    /// </summary>
    /// <param name="name">Updated display name.</param>
    /// <param name="description">Updated description (null to clear).</param>
    /// <param name="frameId">Updated frame reference (null to make frame-flexible).</param>
    /// <param name="lensCatalogItemId">Updated lens reference (null to make lens-flexible).</param>
    /// <param name="comboPrice">Updated bundled price in VND.</param>
    /// <param name="originalTotalPrice">Updated original total for savings display (null to clear).</param>
    public void Update(
        string name,
        string? description,
        Guid? frameId,
        Guid? lensCatalogItemId,
        decimal comboPrice,
        decimal? originalTotalPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Combo package name is required.", nameof(name));

        if (comboPrice <= 0)
            throw new ArgumentException("Combo price must be greater than zero.", nameof(comboPrice));

        if (originalTotalPrice.HasValue && originalTotalPrice.Value <= 0)
            throw new ArgumentException("Original total price must be greater than zero when provided.", nameof(originalTotalPrice));

        Name = name;
        Description = description;
        FrameId = frameId;
        LensCatalogItemId = lensCatalogItemId;
        ComboPrice = comboPrice;
        OriginalTotalPrice = originalTotalPrice;

        SetUpdatedAt();
    }

    /// <summary>
    /// Deactivates the combo package so it is no longer selectable at order creation.
    /// Existing orders referencing this combo are not affected.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Re-activates a previously deactivated combo package.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Calculated savings amount when OriginalTotalPrice is available.
    /// Returns null when savings cannot be determined.
    /// </summary>
    public decimal? SavingsAmount => OriginalTotalPrice.HasValue
        ? OriginalTotalPrice.Value - ComboPrice
        : null;
}
