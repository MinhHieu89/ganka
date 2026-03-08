using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Warranty;

/// <summary>
/// Query to retrieve paginated warranty claims with optional approval status filter.
/// </summary>
public sealed record GetWarrantyClaimsQuery(int? ApprovalStatusFilter, int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated result for warranty claims list.
/// </summary>
public sealed record PagedWarrantyClaimsResult(List<WarrantyClaimSummaryDto> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Wolverine static handler for retrieving a paginated list of warranty claims.
/// Supports filtering by approval status (Pending, Approved, Rejected).
/// </summary>
public static class GetWarrantyClaimsHandler
{
    public static async Task<PagedWarrantyClaimsResult> Handle(
        GetWarrantyClaimsQuery query,
        IWarrantyClaimRepository repository,
        CancellationToken ct)
    {
        var claims = await repository.GetAllAsync(
            query.ApprovalStatusFilter, query.Page, query.PageSize, ct);
        var totalCount = await repository.GetTotalCountAsync(query.ApprovalStatusFilter, ct);

        var items = claims.Select(c => new WarrantyClaimSummaryDto(
            Id: c.Id,
            GlassesOrderId: c.GlassesOrderId,
            PatientName: null,
            ClaimDate: c.ClaimDate,
            Resolution: (int)c.Resolution,
            ApprovalStatus: (int)c.ApprovalStatus,
            RequiresApproval: c.RequiresApproval,
            CreatedAt: c.CreatedAt)).ToList();

        return new PagedWarrantyClaimsResult(
            Items: items,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize);
    }
}
