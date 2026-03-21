using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IDrugBatchRepository"/>.
/// Provides FEFO (First Expiry, First Out) batch queries, expiry alerts,
/// low stock alerts, and stock level computation.
/// </summary>
public sealed class DrugBatchRepository(PharmacyDbContext context) : IDrugBatchRepository
{
    /// <summary>
    /// Gets the current date in Vietnam timezone (UTC+7) for correct FEFO and expiry comparisons.
    /// Uses cross-platform timezone identifier pattern per project standard.
    /// </summary>
    private static DateOnly GetVietnamToday()
    {
        var tzId = OperatingSystem.IsWindows()
            ? "SE Asia Standard Time"
            : "Asia/Ho_Chi_Minh";
        var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
        var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return DateOnly.FromDateTime(vietnamNow);
    }

    public async Task<DrugBatch?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.DrugBatches
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<List<DrugBatch>> GetBatchesForDrugAsync(Guid drugCatalogItemId, CancellationToken ct)
    {
        return await context.DrugBatches
            .AsNoTracking()
            .Where(b => b.DrugCatalogItemId == drugCatalogItemId)
            .OrderByDescending(b => b.ExpiryDate)
            .ToListAsync(ct);
    }

    public async Task<List<DrugBatch>> GetAvailableBatchesFEFOAsync(Guid drugCatalogItemId, CancellationToken ct)
    {
        var today = GetVietnamToday();

        return await context.DrugBatches
            .Where(b => b.DrugCatalogItemId == drugCatalogItemId
                     && b.CurrentQuantity > 0
                     && b.ExpiryDate > today)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalStockAsync(Guid drugCatalogItemId, CancellationToken ct)
    {
        var today = GetVietnamToday();

        return await context.DrugBatches
            .Where(b => b.DrugCatalogItemId == drugCatalogItemId
                     && b.CurrentQuantity > 0
                     && b.ExpiryDate > today)
            .SumAsync(b => b.CurrentQuantity, ct);
    }

    public async Task<List<ExpiryAlertDto>> GetExpiryAlertsAsync(int daysThreshold, CancellationToken ct)
    {
        var today = GetVietnamToday();
        var alertDate = today.AddDays(daysThreshold);
        var todayDayNumber = today.DayNumber;

        // Order by ExpiryDate before projection so EF Core can translate the OrderBy to SQL
        return await context.DrugBatches
            .AsNoTracking()
            .Where(b => b.CurrentQuantity > 0
                     && b.ExpiryDate > today
                     && b.ExpiryDate <= alertDate)
            .OrderBy(b => b.ExpiryDate)
            .Join(
                context.DrugCatalogItems,
                b => b.DrugCatalogItemId,
                d => d.Id,
                (b, d) => new ExpiryAlertDto(
                    d.Id,
                    d.Name,
                    b.BatchNumber,
                    b.ExpiryDate,
                    b.CurrentQuantity,
                    b.ExpiryDate.DayNumber - todayDayNumber))
            .ToListAsync(ct);
    }

    public async Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(CancellationToken ct)
    {
        var today = GetVietnamToday();

        // Group available batch quantities by drug, join with catalog to filter by MinStockLevel
        var stockByDrug = await context.DrugBatches
            .AsNoTracking()
            .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > today)
            .GroupBy(b => b.DrugCatalogItemId)
            .Select(g => new { DrugCatalogItemId = g.Key, TotalStock = g.Sum(b => b.CurrentQuantity) })
            .ToListAsync(ct);

        // Query all active drugs (not just those with MinStockLevel > 0)
        // so that stock=0 drugs always trigger an alert regardless of MinStockLevel setting
        var allActiveDrugs = await context.DrugCatalogItems
            .AsNoTracking()
            .Where(d => d.IsActive)
            .Select(d => new { d.Id, d.Name, d.MinStockLevel })
            .ToListAsync(ct);

        var stockLookup = stockByDrug.ToDictionary(s => s.DrugCatalogItemId, s => s.TotalStock);

        return allActiveDrugs
            .Where(d =>
            {
                var totalStock = stockLookup.TryGetValue(d.Id, out var stock) ? stock : 0;
                // Alert if stock is below minimum threshold OR if stock is zero (out of stock)
                return (d.MinStockLevel > 0 && totalStock < d.MinStockLevel) || totalStock == 0;
            })
            .Select(d => new LowStockAlertDto(
                d.Id,
                d.Name,
                stockLookup.TryGetValue(d.Id, out var stock) ? stock : 0,
                d.MinStockLevel))
            .OrderBy(dto => dto.DrugName)
            .ToList();
    }

    public void Add(DrugBatch batch)
    {
        context.DrugBatches.Add(batch);
    }

    public void AddStockAdjustment(StockAdjustment adjustment)
    {
        context.StockAdjustments.Add(adjustment);
    }
}
