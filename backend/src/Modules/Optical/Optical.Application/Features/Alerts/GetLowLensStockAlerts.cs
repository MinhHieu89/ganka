using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Alerts;

/// <summary>
/// Query to retrieve lens stock entries below minimum stock level.
/// Handler implementation provided in plan 08-21.
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
