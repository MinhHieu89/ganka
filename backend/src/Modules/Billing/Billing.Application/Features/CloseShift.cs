using Billing.Application.Interfaces;
using FluentValidation;
using Shared.Application;
using Shared.Domain;
using Billing.Contracts.Dtos;

namespace Billing.Application.Features;

/// <summary>
/// Command to close the current cashier shift with cash reconciliation.
/// </summary>
public sealed record CloseShiftCommand(decimal ActualCashCount, string? ManagerNote);

/// <summary>
/// Wolverine static handler for closing the current cashier shift.
/// </summary>
public static class CloseShiftHandler
{
    public static Task<Result<CashierShiftDto>> Handle(
        CloseShiftCommand command,
        ICashierShiftRepository shiftRepository,
        IUnitOfWork unitOfWork,
        IValidator<CloseShiftCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
