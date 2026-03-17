using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Query to get closed shifts for the current user's branch with pagination.
/// </summary>
public sealed record GetShiftHistoryQuery(int Page = 1, int PageSize = 20);

/// <summary>
/// Wolverine static handler for retrieving closed shift history.
/// </summary>
public static class GetShiftHistoryHandler
{
    public static async Task<Result<ShiftHistoryResult>> Handle(
        GetShiftHistoryQuery query,
        ICashierShiftRepository shiftRepository,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var branchId = new BranchId(currentUser.BranchId);
        var (shifts, totalCount) = await shiftRepository.GetClosedAsync(branchId, query.Page, query.PageSize, ct);

        var items = shifts.Select(OpenShiftHandler.MapToDto).ToList();
        return new ShiftHistoryResult(items, totalCount);
    }
}

public sealed record ShiftHistoryResult(List<CashierShiftDto> Items, int TotalCount);
