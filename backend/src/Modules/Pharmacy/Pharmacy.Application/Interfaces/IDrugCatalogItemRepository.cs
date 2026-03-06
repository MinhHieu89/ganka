using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

/// <summary>
/// Repository interface for DrugCatalogItem persistence operations.
/// SearchAsync returns DTOs for cross-module consumption via Wolverine bus.
/// </summary>
public interface IDrugCatalogItemRepository
{
    /// <summary>
    /// Searches active drug catalog items by name, Vietnamese name, or generic name.
    /// Returns top 20 matching items as DTOs.
    /// </summary>
    Task<List<DrugCatalogItemDto>> SearchAsync(string searchTerm, CancellationToken ct);

    /// <summary>
    /// Gets a drug catalog item by ID (returns domain entity for mutation).
    /// </summary>
    Task<DrugCatalogItem?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all active drug catalog items.
    /// </summary>
    Task<List<DrugCatalogItem>> GetAllActiveAsync(CancellationToken ct);

    /// <summary>
    /// Adds a new drug catalog item to the change tracker.
    /// </summary>
    void Add(DrugCatalogItem item);

    /// <summary>
    /// Marks a drug catalog item as modified in the change tracker.
    /// </summary>
    void Update(DrugCatalogItem item);

    /// <summary>
    /// Gets all active drug catalog items with aggregated inventory data from DrugBatches.
    /// Returns inventory summary including TotalStock, BatchCount, IsLowStock, and HasExpiryAlert.
    /// Used for the pharmacy inventory management list view.
    /// </summary>
    Task<List<DrugInventoryDto>> GetAllWithInventoryAsync(int expiryAlertDays, CancellationToken ct);
}
