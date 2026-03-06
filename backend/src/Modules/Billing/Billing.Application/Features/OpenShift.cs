using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to open a new cashier shift.
/// </summary>
public sealed record OpenShiftCommand(decimal OpeningBalance, Guid? ShiftTemplateId);

/// <summary>
/// Validator for <see cref="OpenShiftCommand"/>.
/// </summary>
public class OpenShiftCommandValidator : AbstractValidator<OpenShiftCommand>
{
    public OpenShiftCommandValidator()
    {
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0)
            .WithMessage("Opening balance must be zero or positive.");
    }
}

/// <summary>
/// Wolverine static handler for opening a cashier shift.
/// Prevents duplicate open shifts per branch.
/// </summary>
public static class OpenShiftHandler
{
    public static async Task<Result<CashierShiftDto>> Handle(
        OpenShiftCommand command,
        ICashierShiftRepository shiftRepository,
        IUnitOfWork unitOfWork,
        IValidator<OpenShiftCommand> validator,
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

        // Prevent duplicate open shifts per branch
        var existingShift = await shiftRepository.GetCurrentOpenAsync(branchId, ct);
        if (existingShift is not null)
        {
            return Result.Failure<CashierShiftDto>(
                Error.Conflict("A shift is already open for this branch."));
        }

        var shift = CashierShift.Create(
            cashierId: currentUser.UserId,
            cashierName: currentUser.Email,
            openingBalance: command.OpeningBalance,
            shiftTemplateId: command.ShiftTemplateId,
            branchId: branchId);

        shiftRepository.Add(shift);
        await unitOfWork.SaveChangesAsync(ct);

        return MapToDto(shift);
    }

    internal static CashierShiftDto MapToDto(CashierShift shift) =>
        new(
            Id: shift.Id,
            CashierId: shift.CashierId,
            CashierName: shift.CashierName,
            ShiftTemplateId: shift.ShiftTemplateId,
            Status: (int)shift.Status,
            OpenedAt: shift.OpenedAt,
            ClosedAt: shift.ClosedAt,
            OpeningBalance: shift.OpeningBalance,
            ExpectedCashAmount: shift.ExpectedCashAmount,
            CashReceived: shift.CashReceived,
            CashRefunds: shift.CashRefunds,
            ActualCashCount: shift.ActualCashCount,
            Discrepancy: shift.Discrepancy,
            ManagerNote: shift.ManagerNote,
            TotalRevenue: shift.TotalRevenue,
            TransactionCount: shift.TransactionCount);
}
