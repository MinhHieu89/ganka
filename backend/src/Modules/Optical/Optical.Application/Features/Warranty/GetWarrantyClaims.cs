using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Warranty;

/// <summary>
/// Query to retrieve paginated warranty claims with optional approval status filter.
/// Handler implementation provided in plan 08-20.
/// </summary>
public sealed record GetWarrantyClaimsQuery(int? ApprovalStatusFilter, int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated result for warranty claims list.
/// </summary>
public sealed record PagedWarrantyClaimsResult(List<WarrantyClaimSummaryDto> Items, int TotalCount, int Page, int PageSize);
