using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Represents a drug supplier. A supplier provides drugs to the clinic at negotiated prices.
/// Linked to DrugCatalogItems via SupplierDrugPrice for default purchase pricing,
/// and to DrugBatches for batch-level supply traceability.
/// </summary>
public class Supplier : AggregateRoot, IAuditable
{
    /// <summary>Supplier company or business name (e.g., "Công ty Dược Phẩm ABC")</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>General contact information or address (optional)</summary>
    public string? ContactInfo { get; private set; }

    /// <summary>Phone number of the supplier representative (optional)</summary>
    public string? Phone { get; private set; }

    /// <summary>Email address for ordering or correspondence (optional)</summary>
    public string? Email { get; private set; }

    /// <summary>Whether the supplier is currently active. Inactive suppliers are hidden from selection.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Flags indicating what types of products this supplier provides (Drug, Optical, or both).</summary>
    public SupplierType SupplierTypes { get; private set; } = SupplierType.Drug;

    /// <summary>Private constructor for EF Core materialization.</summary>
    private Supplier() { }

    /// <summary>
    /// Factory method for creating a new supplier.
    /// </summary>
    /// <param name="name">Required supplier name.</param>
    /// <param name="contactInfo">Optional address or general contact details.</param>
    /// <param name="phone">Optional phone number.</param>
    /// <param name="email">Optional email address.</param>
    /// <param name="branchId">The branch this supplier is registered under.</param>
    /// <param name="supplierTypes">Flags indicating supplier type(s). Defaults to Drug for backward compatibility.</param>
    public static Supplier Create(
        string name,
        string? contactInfo,
        string? phone,
        string? email,
        BranchId branchId,
        SupplierType supplierTypes = SupplierType.Drug)
    {
        var supplier = new Supplier
        {
            Name = name,
            ContactInfo = contactInfo,
            Phone = phone,
            Email = email,
            IsActive = true,
            SupplierTypes = supplierTypes
        };

        supplier.SetBranchId(branchId);
        return supplier;
    }

    /// <summary>
    /// Updates the editable fields of the supplier.
    /// </summary>
    public void Update(string name, string? contactInfo, string? phone, string? email)
    {
        Name = name;
        ContactInfo = contactInfo;
        Phone = phone;
        Email = email;

        SetUpdatedAt();
    }

    /// <summary>Sets the supplier type flags. Use bitwise OR for multiple types (e.g., Drug | Optical).</summary>
    public void SetSupplierTypes(SupplierType types)
    {
        SupplierTypes = types;
        SetUpdatedAt();
    }

    /// <summary>
    /// Soft-deactivates the supplier, hiding it from selection in new stock imports.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Re-activates a previously deactivated supplier.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
