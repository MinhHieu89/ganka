using Billing.Application.Interfaces;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Domain;
using Wolverine;

namespace Billing.Application.Features;

/// <summary>
/// Command to approve a requested refund with manager PIN verification.
/// ManagerId is included for PIN verification via cross-module query (same pattern as ApproveDiscountCommand).
/// </summary>
public sealed record ApproveRefundCommand(
    Guid InvoiceId,
    Guid RefundId,
    Guid ManagerId,
    string ManagerPin);

/// <summary>
/// Validator for <see cref="ApproveRefundCommand"/>.
/// </summary>
public class ApproveRefundCommandValidator : AbstractValidator<ApproveRefundCommand>
{
    public ApproveRefundCommandValidator()
    {
        RuleFor(x => x.RefundId).NotEmpty();
        RuleFor(x => x.ManagerPin).NotEmpty().WithMessage("Manager PIN is required.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="ApproveRefundCommand"/>.
/// Verifies manager PIN via cross-module query, validates refund is in Requested status,
/// then approves the refund.
/// </summary>
public static class ApproveRefundHandler
{
    public static async Task<Result> Handle(
        ApproveRefundCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result.Failure(Error.NotFound("Invoice", command.InvoiceId));

        var refund = invoice.Refunds.FirstOrDefault(r => r.Id == command.RefundId);
        if (refund is null)
            return Result.Failure(Error.NotFound("Refund", command.RefundId));

        // Validate refund status is Requested before attempting approval
        if (refund.Status != RefundStatus.Requested)
            return Result.Failure(Error.Validation("Only refunds in Requested status can be approved."));

        // Verify manager PIN via cross-module query to Auth module
        var pinResponse = await messageBus.InvokeAsync<VerifyManagerPinResponse>(
            new VerifyManagerPinQuery(command.ManagerId, command.ManagerPin), cancellationToken);

        if (!pinResponse.IsValid)
            return Result.Failure(Error.Validation("Invalid manager PIN."));

        // Approve the refund (domain method validates status is Requested)
        // Invoice is tracked via GetByIdAsync -- no Update() needed
        refund.Approve(command.ManagerId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
