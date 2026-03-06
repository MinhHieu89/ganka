using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Query to get the currently open shift for the current user's branch.
/// </summary>
public sealed record GetCurrentShiftQuery();

/// <summary>
/// Wolverine static handler for retrieving the current open shift.
/// Returns null if no shift is currently open.
/// </summary>
public static class GetCurrentShiftHandler
{
    public static async Task<Result<CashierShiftDto?>> Handle(
        GetCurrentShiftQuery query,
        ICashierShiftRepository shiftRepository,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var branchId = new BranchId(currentUser.BranchId);
        var shift = await shiftRepository.GetCurrentOpenAsync(branchId, ct);

        if (shift is null)
        {
            return Result.Success<CashierShiftDto?>(null);
        }

        return Result.Success<CashierShiftDto?>(OpenShiftHandler.MapToDto(shift));
    }
}
