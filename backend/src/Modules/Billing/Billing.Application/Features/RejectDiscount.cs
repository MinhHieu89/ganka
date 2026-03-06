using Billing.Application.Interfaces;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Domain;
using Wolverine;

namespace Billing.Application.Features;

/// <summary>
/// Command to reject a pending discount with manager PIN verification and rejection reason.
/// ManagerId is included for PIN verification via cross-module query (same pattern as ApproveDiscountCommand).
/// </summary>
public sealed record RejectDiscountCommand(
    Guid InvoiceId,
    Guid DiscountId,
    string RejectionReason,
    Guid ManagerId,
    string ManagerPin);

/// <summary>
/// Validator for <see cref="RejectDiscountCommand"/>.
/// </summary>
public class RejectDiscountCommandValidator : AbstractValidator<RejectDiscountCommand>
{
    public RejectDiscountCommandValidator()
    {
        RuleFor(x => x.DiscountId).NotEmpty();
        RuleFor(x => x.RejectionReason).NotEmpty().WithMessage("Rejection reason is required.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="RejectDiscountCommand"/>.
/// Verifies manager PIN via cross-module query, then rejects discount with reason
/// and recalculates invoice totals to exclude the rejected discount.
/// </summary>
public static class RejectDiscountHandler
{
    public static async Task<Result> Handle(
        RejectDiscountCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result.Failure(Error.NotFound("Invoice", command.InvoiceId));

        var discount = invoice.Discounts.FirstOrDefault(d => d.Id == command.DiscountId);
        if (discount is null)
            return Result.Failure(Error.NotFound("Discount", command.DiscountId));

        if (discount.ApprovalStatus != ApprovalStatus.Pending)
            return Result.Failure(Error.Validation("Discount has already been processed."));

        // Verify manager PIN via cross-module query to Auth module
        var pinResponse = await messageBus.InvokeAsync<VerifyManagerPinResponse>(
            new VerifyManagerPinQuery(command.ManagerId, command.ManagerPin), cancellationToken);

        if (!pinResponse.IsValid)
            return Result.Failure(Error.Validation("Invalid manager PIN."));

        // Reject the discount with reason and recalculate invoice totals
        discount.Reject(command.ManagerId, command.RejectionReason);
        invoice.RecalculateAfterDiscountApproval();

        invoiceRepository.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
