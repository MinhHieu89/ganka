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
    /// Returns a paginated list of active drug catalog items, optionally filtered by search term.
    /// Returns a tuple of (items on page, total count of matching items).
    /// </summary>
    Task<(List<DrugCatalogItemDto> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, string? search, CancellationToken ct);

    /// <summary>
    /// Gets all active drug catalog items with aggregated inventory data from DrugBatches.
    /// Returns inventory summary including TotalStock, BatchCount, IsLowStock, and HasExpiryAlert.
    /// Used for the pharmacy inventory management list view.
    /// </summary>
    Task<List<DrugInventoryDto>> GetAllWithInventoryAsync(int expiryAlertDays, CancellationToken ct);

    /// <summary>
    /// Gets selling prices and Vietnamese names for the specified catalog item IDs.
    /// Used by the Billing module to look up pricing when creating line items from prescriptions.
    /// </summary>
    Task<List<DrugCatalogPriceDto>> GetPricesByIdsAsync(List<Guid> catalogItemIds, CancellationToken ct);
}
