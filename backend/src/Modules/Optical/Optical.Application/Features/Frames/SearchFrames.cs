using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Query to search frames by term and filters.
/// Handler implementation provided in plan 08-16.
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
