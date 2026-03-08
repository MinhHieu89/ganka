using Microsoft.EntityFrameworkCore;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;

namespace Optical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ILensCatalogRepository"/>.
/// Provides data access for LensCatalogItem aggregates with their LensStockEntry children.
/// All GetByIdAsync calls eagerly include StockEntries for full aggregate loading.
/// </summary>
public sealed class LensCatalogRepository(OpticalDbContext context) : ILensCatalogRepository
{
    /// <summary>
    /// Gets a lens catalog item by ID with all stock entries eagerly loaded.
    /// Returns null if not found.
    /// </summary>
    public async Task<LensCatalogItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.LensCatalogItems
            .Include(x => x.StockEntries)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Gets all lens catalog items with stock entries, optionally including inactive items.
    /// Ordered by Brand, then Name for consistent display.
    /// </summary>
    public async Task<List<LensCatalogItem>> GetAllAsync(bool includeInactive, CancellationToken ct)
    {
        var query = context.LensCatalogItems
            .Include(x => x.StockEntries)
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Brand)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Finds the stock entry for a specific lens power combination (SPH/CYL/ADD) within a catalog item.
    /// Returns null when no stock entry exists for the given power combination.
    /// Used to check existing stock before adjusting lens inventory.
    /// </summary>
    public async Task<LensStockEntry?> GetStockEntryAsync(
        Guid catalogItemId,
        decimal sph,
        decimal cyl,
        decimal? add,
        CancellationToken ct)
    {
        return await context.LensStockEntries
            .FirstOrDefaultAsync(e =>
                e.LensCatalogItemId == catalogItemId &&
                e.Sph == sph &&
                e.Cyl == cyl &&
                e.Add == add,
                ct);
    }

    /// <summary>
    /// Adds a new lens catalog item to the EF Core change tracker.
    /// </summary>
    public void Add(LensCatalogItem item)
    {
        context.LensCatalogItems.Add(item);
    }
}
