using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Query to search frames by term and filters.
/// </summary>
public sealed record SearchFramesQuery(
    string? SearchTerm,
    int? Material,
    int? FrameType,
    int? Gender,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// Paginated search result for frames.
/// </summary>
public sealed record FrameSearchResult(List<FrameSummaryDto> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Wolverine static handler for searching frames with optional text and enum filters.
/// Calls repository.SearchAsync + GetTotalCountAsync and maps inline (no AutoMapper).
/// </summary>
public static class SearchFramesHandler
{
    public static async Task<Result<FrameSearchResult>> Handle(
        SearchFramesQuery query,
        IFrameRepository repository,
        CancellationToken ct)
    {
        var frames = await repository.SearchAsync(
            query.SearchTerm,
            query.Material,
            query.FrameType,
            query.Gender,
            query.Page,
            query.PageSize,
            ct);

        var totalCount = await repository.GetTotalCountAsync(
            query.SearchTerm,
            query.Material,
            query.FrameType,
            query.Gender,
            ct);

        var items = frames.Select(GetFramesHandler.ToSummaryDto).ToList();

        return new FrameSearchResult(
            Items: items,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize);
    }
}
