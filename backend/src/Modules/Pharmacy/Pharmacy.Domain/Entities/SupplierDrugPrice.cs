using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Junction entity linking a Supplier to a DrugCatalogItem with a default purchase price.
/// Used to auto-fill the purchase price when a pharmacist creates a stock import from this supplier.
/// One supplier can have different default prices for different drugs.
/// </summary>
public class SupplierDrugPrice : Entity
{
    /// <summary>Foreign key to the Supplier.</summary>
    public Guid SupplierId { get; private set; }

    /// <summary>Foreign key to the DrugCatalogItem.</summary>
    public Guid DrugCatalogItemId { get; private set; }

    /// <summary>
    /// The default purchase price (VND) this supplier charges for this drug.
    /// Auto-populated when the pharmacist selects this supplier during stock import.
    /// </summary>
    public decimal DefaultPurchasePrice { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private SupplierDrugPrice() { }

    /// <summary>
    /// Factory method for creating a new supplier-drug price entry.
    /// </summary>
    /// <param name="supplierId">The supplier providing this drug.</param>
    /// <param name="drugCatalogItemId">The drug catalog item being priced.</param>
    /// <param name="defaultPurchasePrice">The default purchase price in VND (must be non-negative).</param>
    public static SupplierDrugPrice Create(Guid supplierId, Guid drugCatalogItemId, decimal defaultPurchasePrice)
    {
        if (defaultPurchasePrice < 0)
            throw new ArgumentException("Default purchase price cannot be negative.", nameof(defaultPurchasePrice));

        return new SupplierDrugPrice
        {
            SupplierId = supplierId,
            DrugCatalogItemId = drugCatalogItemId,
            DefaultPurchasePrice = defaultPurchasePrice
        };
    }

    /// <summary>
    /// Updates the default purchase price for this supplier-drug combination.
    /// </summary>
    /// <param name="newPrice">New purchase price in VND (must be non-negative).</param>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Purchase price cannot be negative.", nameof(newPrice));

        DefaultPurchasePrice = newPrice;
        SetUpdatedAt();
    }
}
