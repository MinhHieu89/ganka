using Shared.Domain;

namespace Optical.Application.Features.Lenses;

/// <summary>
/// Command to adjust stock for a specific lens power combination.
/// Handler implementation provided in plan 08-17.
/// </summary>
public sealed record AdjustLensStockCommand(
    Guid LensCatalogItemId,
    decimal Sph,
    decimal Cyl,
    decimal? Add,
    int QuantityChange,
    string Reason);
