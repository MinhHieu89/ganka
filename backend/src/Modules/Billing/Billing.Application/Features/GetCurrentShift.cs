using Billing.Application.Interfaces;
using Shared.Application;
using Shared.Domain;
using Billing.Contracts.Dtos;

namespace Billing.Application.Features;

/// <summary>
/// Query to get the currently open shift for the current user's branch.
/// </summary>
public sealed record GetCurrentShiftQuery();

/// <summary>
/// Wolverine static handler for retrieving the current open shift.
/// </summary>
public static class GetCurrentShiftHandler
{
    public static Task<Result<CashierShiftDto?>> Handle(
        GetCurrentShiftQuery query,
        ICashierShiftRepository shiftRepository,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
