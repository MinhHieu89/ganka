using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Alerts;

/// <summary>
/// Query to retrieve lens stock entries below minimum stock level.
/// </summary>
public sealed record GetLowLensStockAlertsQuery();

/// <summary>
/// DTO for a low lens stock alert.
/// </summary>
public sealed record LowLensStockAlertDto(
    Guid LensCatalogItemId,
    string LensName,
    string Brand,
    decimal Sph,
    decimal Cyl,
    decimal? Add,
    int CurrentStock,
    int MinStockLevel);

/// <summary>
/// Wolverine static handler for retrieving low lens stock alerts.
/// Fetches all active lens catalog items with stock entries, then filters
/// to those where Quantity is below the MinStockLevel threshold.
/// </summary>
public static class GetLowLensStockAlertsHandler
{
    public static async Task<Result<List<LowLensStockAlertDto>>> Handle(
        GetLowLensStockAlertsQuery query,
        ILensCatalogRepository repository,
        CancellationToken ct)
    {
        var catalogItems = await repository.GetAllAsync(includeInactive: false, ct);

        var alerts = new List<LowLensStockAlertDto>();

        foreach (var item in catalogItems)
        {
            foreach (var entry in item.StockEntries)
            {
                if (entry.IsLowStock)
                {
                    alerts.Add(new LowLensStockAlertDto(
                        LensCatalogItemId: item.Id,
                        LensName: item.Name,
                        Brand: item.Brand,
                        Sph: entry.Sph,
                        Cyl: entry.Cyl,
                        Add: entry.Add,
                        CurrentStock: entry.Quantity,
                        MinStockLevel: entry.MinStockLevel));
                }
            }
        }

        return alerts;
    }
}
