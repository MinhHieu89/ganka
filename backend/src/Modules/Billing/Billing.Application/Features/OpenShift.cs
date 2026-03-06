using Billing.Application.Interfaces;
using FluentValidation;
using Shared.Application;
using Shared.Domain;
using Billing.Contracts.Dtos;

namespace Billing.Application.Features;

/// <summary>
/// Command to open a new cashier shift.
/// </summary>
public sealed record OpenShiftCommand(decimal OpeningBalance, Guid? ShiftTemplateId);

/// <summary>
/// Wolverine static handler for opening a cashier shift.
/// </summary>
public static class OpenShiftHandler
{
    public static Task<Result<CashierShiftDto>> Handle(
        OpenShiftCommand command,
        ICashierShiftRepository shiftRepository,
        IUnitOfWork unitOfWork,
        IValidator<OpenShiftCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
