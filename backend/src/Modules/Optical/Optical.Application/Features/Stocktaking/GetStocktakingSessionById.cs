using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Query to retrieve a single stocktaking session with all scanned items.
/// Handler implementation provided in plan 08-20.
/// </summary>
public sealed record GetStocktakingSessionByIdQuery(Guid Id);

/// <summary>
/// Full stocktaking session detail including all scanned items.
/// </summary>
public sealed record StocktakingSessionDetailDto(
    StocktakingSessionDto Session,
    List<StocktakingItemDto> Items);
