using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Query to retrieve paginated list of stocktaking sessions.
/// Handler implementation provided in plan 08-20.
/// </summary>
public sealed record GetStocktakingSessionsQuery(int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated result for stocktaking sessions list.
/// </summary>
public sealed record PagedStocktakingSessionsResult(List<StocktakingSessionDto> Items, int TotalCount, int Page, int PageSize);
