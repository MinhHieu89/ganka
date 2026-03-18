using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Query to retrieve paginated list of stocktaking sessions.
/// </summary>
public sealed record GetStocktakingSessionsQuery(int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated result for stocktaking sessions list.
/// </summary>
public sealed record PagedStocktakingSessionsResult(List<StocktakingSessionDto> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Wolverine static handler for retrieving paginated stocktaking sessions.
/// </summary>
public static class GetStocktakingSessionsHandler
{
    public static async Task<Result<PagedStocktakingSessionsResult>> Handle(
        GetStocktakingSessionsQuery query,
        IStocktakingRepository repository,
        CancellationToken ct)
    {
        var sessions = await repository.GetAllAsync(query.Page, query.PageSize, ct);
        var totalCount = await repository.GetTotalCountAsync(ct);

        var items = sessions.Select(s => new StocktakingSessionDto(
            Id: s.Id,
            Name: s.Name,
            Status: (int)s.Status,
            StartedById: s.StartedById,
            StartedByName: null,
            CreatedAt: s.CreatedAt,
            CompletedAt: s.CompletedAt,
            TotalItemsScanned: s.TotalItemsScanned,
            DiscrepancyCount: s.Items.Count(i => i.PhysicalCount != i.SystemCount),
            Notes: s.Notes)).ToList();

        return new PagedStocktakingSessionsResult(items, totalCount, query.Page, query.PageSize);
    }
}
