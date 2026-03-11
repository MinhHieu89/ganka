using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

/// <summary>
/// Repository interface for Supplier persistence operations.
/// Includes methods for supplier CRUD and SupplierDrugPrice queries.
/// </summary>
public interface ISupplierRepository
{
    /// <summary>
    /// Gets a supplier by ID (returns domain entity for mutation).
    /// </summary>
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all active suppliers.
    /// </summary>
    Task<List<Supplier>> GetAllActiveAsync(CancellationToken ct);

    /// <summary>
    /// Gets all suppliers (both active and inactive).
    /// </summary>
    Task<List<Supplier>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Gets all drug prices for a specific supplier.
    /// </summary>
    Task<List<SupplierDrugPriceDto>> GetSupplierDrugPricesAsync(Guid supplierId, CancellationToken ct);

    /// <summary>
    /// Gets the default purchase price a supplier offers for a specific drug.
    /// Returns null if no price entry exists.
    /// </summary>
    Task<decimal?> GetDefaultPriceAsync(Guid supplierId, Guid drugCatalogItemId, CancellationToken ct);

    /// <summary>
    /// Adds a new supplier to the change tracker.
    /// </summary>
    void Add(Supplier supplier);

    /// <summary>
    /// Adds a new SupplierDrugPrice to the change tracker.
    /// </summary>
    void AddSupplierDrugPrice(SupplierDrugPrice price);

    /// <summary>
    /// Marks a SupplierDrugPrice as modified in the change tracker.
    /// </summary>
    void UpdateSupplierDrugPrice(SupplierDrugPrice price);
}
