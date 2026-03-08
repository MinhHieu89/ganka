using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Query to retrieve paginated list of glasses orders with optional status filter.
/// Handler implementation provided in plan 08-18.
/// </summary>
public sealed record GetGlassesOrdersQuery(int? StatusFilter, int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated result for glasses orders list.
/// </summary>
public sealed record PagedGlassesOrdersResult(List<GlassesOrderSummaryDto> Items, int TotalCount, int Page, int PageSize);
