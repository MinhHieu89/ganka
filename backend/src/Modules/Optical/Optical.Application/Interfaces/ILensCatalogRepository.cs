using Optical.Domain.Entities;

namespace Optical.Application.Interfaces;

/// <summary>
/// Repository interface for LensCatalogItem persistence operations.
/// Provides catalog management for lens types and their per-power stock entries.
/// All GetByIdAsync calls include StockEntries for full aggregate loading.
/// </summary>
public interface ILensCatalogRepository
{
    /// <summary>
    /// Gets a lens catalog item by its unique identifier.
    /// Eagerly includes all <see cref="LensStockEntry"/> children.
    /// Returns null if not found.
    /// </summary>
    Task<LensCatalogItem?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all lens catalog items, optionally including inactive entries.
    /// Includes StockEntries for each item.
    /// Ordered by Brand, then Name.
    /// </summary>
    Task<List<LensCatalogItem>> GetAllAsync(bool includeInactive, CancellationToken ct);

    /// <summary>
    /// Finds the stock entry for a specific lens power combination within a catalog item.
    /// Returns null when no stock entry exists for the given SPH/CYL/ADD combination.
    /// Used to check existing stock before deducting or adding stock entries.
    /// </summary>
    Task<LensStockEntry?> GetStockEntryAsync(
        Guid catalogItemId,
        decimal sph,
        decimal cyl,
        decimal? add,
        CancellationToken ct);

    /// <summary>
    /// Adds a new lens catalog item to the EF Core change tracker.
    /// Call IUnitOfWork.SaveChangesAsync to persist.
    /// </summary>
    void Add(LensCatalogItem item);
}
