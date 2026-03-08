using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Query to retrieve paginated list of frames.
/// Handler implementation provided in plan 08-16.
/// </summary>
public sealed record GetFramesQuery(bool IncludeInactive = false, int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated result for frame list queries.
/// </summary>
public sealed record PagedFramesResult(List<FrameSummaryDto> Items, int TotalCount, int Page, int PageSize);
