using Shared.Domain;

namespace Optical.Application.Features.Lenses;

/// <summary>
/// Command to create a new lens catalog item.
/// Handler implementation provided in plan 08-17.
/// </summary>
public sealed record CreateLensCatalogItemCommand(
    string Brand,
    string Name,
    string LensType,
    int Material,
    int AvailableCoatings,
    decimal SellingPrice,
    decimal CostPrice,
    Guid? PreferredSupplierId);
