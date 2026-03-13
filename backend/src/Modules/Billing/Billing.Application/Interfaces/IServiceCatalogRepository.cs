using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

/// <summary>
/// Repository interface for ServiceCatalogItem persistence.
/// Supports lookup by code for auto-pricing and full CRUD for admin management.
/// </summary>
public interface IServiceCatalogRepository
{
    /// <summary>
    /// Gets a service catalog item by ID.
    /// </summary>
    Task<ServiceCatalogItem?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets an active service catalog item by its unique code (e.g., CONSULTATION).
    /// Returns null if not found or inactive.
    /// </summary>
    Task<ServiceCatalogItem?> GetActiveByCodeAsync(string code, CancellationToken ct);

    /// <summary>
    /// Gets all service catalog items. Optionally includes inactive items.
    /// </summary>
    Task<List<ServiceCatalogItem>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Adds a new service catalog item to the change tracker.
    /// </summary>
    void Add(ServiceCatalogItem item);

    /// <summary>
    /// Marks an existing service catalog item as modified in the change tracker.
    /// </summary>
    void Update(ServiceCatalogItem item);
}
