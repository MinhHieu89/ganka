using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to close the current cashier shift with cash reconciliation.
/// </summary>
public sealed record CloseShiftCommand(decimal ActualCashCount, string? ManagerNote);

/// <summary>
/// Validator for <see cref="CloseShiftCommand"/>.
/// </summary>
public class CloseShiftCommandValidator : AbstractValidator<CloseShiftCommand>
{
    public CloseShiftCommandValidator()
    {
        RuleFor(x => x.ActualCashCount).GreaterThanOrEqualTo(0)
            .WithMessage("Actual cash count must be zero or positive.");
    }
}

/// <summary>
/// Wolverine static handler for closing the current cashier shift.
/// Locks the shift to prevent new payment assignments, then closes with cash reconciliation.
/// </summary>
public static class CloseShiftHandler
{
    public static async Task<Result<CashierShiftDto>> Handle(
        CloseShiftCommand command,
        ICashierShiftRepository shiftRepository,
        IUnitOfWork unitOfWork,
        IValidator<CloseShiftCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<CashierShiftDto>(Error.ValidationWithDetails(errors));
        }

        var branchId = new BranchId(currentUser.BranchId);

        var shift = await shiftRepository.GetCurrentOpenAsync(branchId, ct);
        if (shift is null)
        {
            return Result.Failure<CashierShiftDto>(
                Error.NotFound("CashierShift", $"No open shift found for branch {branchId.Value}"));
        }

        // Lock prevents new payment assignments, then close calculates discrepancy
        shift.LockForClose();
        shift.Close(command.ActualCashCount, command.ManagerNote);

        shiftRepository.Update(shift);
        await unitOfWork.SaveChangesAsync(ct);

        return OpenShiftHandler.MapToDto(shift);
    }
}
