using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Repositories;


/// <summary>
/// EF Core implementation of <see cref="IDrugCatalogItemRepository"/>.
/// Leverages Vietnamese_CI_AI collation on Name, NameVi, and GenericName columns
/// for accent-insensitive, case-insensitive drug search at the database level.
/// </summary>
public sealed class DrugCatalogItemRepository : IDrugCatalogItemRepository
{
    private readonly PharmacyDbContext _dbContext;

    public DrugCatalogItemRepository(PharmacyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DrugCatalogItemDto>> SearchAsync(string searchTerm, CancellationToken ct)
    {
        return await _dbContext.DrugCatalogItems
            .AsNoTracking()
            .Where(d => d.IsActive &&
                (d.Name.Contains(searchTerm) ||
                 d.NameVi.Contains(searchTerm) ||
                 d.GenericName.Contains(searchTerm)))
            .OrderBy(d => d.Name)
            .Take(20)
            .Select(d => new DrugCatalogItemDto(
                d.Id,
                d.Name,
                d.NameVi,
                d.GenericName,
                (int)d.Form,
                d.Strength,
                (int)d.Route,
                d.Unit,
                d.DefaultDosageTemplate,
                d.IsActive,
                d.SellingPrice,
                d.MinStockLevel))
            .ToListAsync(ct);
    }

    public async Task<DrugCatalogItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbContext.DrugCatalogItems
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<List<DrugCatalogItem>> GetAllActiveAsync(CancellationToken ct)
    {
        return await _dbContext.DrugCatalogItems
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public void Add(DrugCatalogItem item)
    {
        _dbContext.DrugCatalogItems.Add(item);
    }

    public void Update(DrugCatalogItem item)
    {
        _dbContext.DrugCatalogItems.Update(item);
    }

    /// <summary>
    /// Returns all active drug catalog items joined with their batch inventory data.
    /// Computes TotalStock (sum of non-expired batch quantities), BatchCount, IsLowStock,
    /// and HasExpiryAlert (any batch expiring within expiryAlertDays).
    /// Uses two-step query to work around EF Core GroupBy translation limitations.
    /// </summary>
    public async Task<List<DrugInventoryDto>> GetAllWithInventoryAsync(int expiryAlertDays, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var alertDate = today.AddDays(expiryAlertDays);

        // Step 1: Load all active catalog items
        var drugs = await _dbContext.DrugCatalogItems
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        var drugIds = drugs.Select(d => d.Id).ToList();

        // Step 2: Aggregate batch data for all drugs in one query
        var batchAggregates = await _dbContext.DrugBatches
            .AsNoTracking()
            .Where(b => drugIds.Contains(b.DrugCatalogItemId))
            .GroupBy(b => b.DrugCatalogItemId)
            .Select(g => new
            {
                DrugCatalogItemId = g.Key,
                TotalStock = g.Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > today)
                              .Sum(b => (int?)b.CurrentQuantity) ?? 0,
                BatchCount = g.Count(),
                HasExpiryAlert = g.Any(b => b.CurrentQuantity > 0
                                         && b.ExpiryDate > today
                                         && b.ExpiryDate <= alertDate)
            })
            .ToListAsync(ct);

        var batchLookup = batchAggregates.ToDictionary(x => x.DrugCatalogItemId);

        return drugs.Select(d =>
        {
            var agg = batchLookup.TryGetValue(d.Id, out var found) ? found : null;
            var totalStock = agg?.TotalStock ?? 0;
            var batchCount = agg?.BatchCount ?? 0;
            var hasExpiryAlert = agg?.HasExpiryAlert ?? false;
            var isLowStock = d.MinStockLevel > 0 && totalStock < d.MinStockLevel;

            return new DrugInventoryDto(
                d.Id,
                d.Name,
                d.NameVi,
                d.GenericName,
                d.Unit,
                (int)d.Form,
                (int)d.Route,
                d.SellingPrice,
                d.MinStockLevel,
                totalStock,
                batchCount,
                isLowStock,
                hasExpiryAlert);
        }).ToList();
    }
}
