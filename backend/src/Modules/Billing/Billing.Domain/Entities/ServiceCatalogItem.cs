using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Configurable pricing entry for clinical services (consultation, follow-up, etc.).
/// Used by billing event handlers to auto-price invoice line items when visits are created.
/// Implements IAuditable for automatic field-level change tracking (FIN-09).
/// </summary>
public class ServiceCatalogItem : AggregateRoot, IAuditable
{
    /// <summary>Unique service code (e.g., CONSULTATION, FOLLOWUP).</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>English display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Vietnamese display name.</summary>
    public string NameVi { get; private set; } = string.Empty;

    /// <summary>Price in VND (whole numbers, no decimals).</summary>
    public decimal Price { get; private set; }

    /// <summary>Whether this service is currently available for billing.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Optional notes about this service.</summary>
    public string? Description { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private ServiceCatalogItem() { }

    /// <summary>
    /// Factory method for creating a new service catalog item.
    /// </summary>
    public static ServiceCatalogItem Create(
        string code,
        string name,
        string nameVi,
        decimal price,
        BranchId branchId,
        string? description = null)
    {
        var item = new ServiceCatalogItem
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            NameVi = nameVi,
            Price = price,
            IsActive = true,
            Description = description
        };

        item.SetBranchId(branchId);
        return item;
    }

    /// <summary>
    /// Updates the service catalog item details. Price changes are automatically
    /// tracked by the audit interceptor (FIN-09).
    /// </summary>
    public void Update(string name, string nameVi, decimal price, string? description)
    {
        Name = name;
        NameVi = nameVi;
        Price = price;
        Description = description;
        SetUpdatedAt();
    }

    /// <summary>
    /// Deactivates the service catalog item (soft removal, no deletion).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Reactivates a previously deactivated service catalog item.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
