using Billing.Application.Interfaces;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to reject a pending discount with rejection reason.
/// </summary>
public sealed record RejectDiscountCommand(
    Guid InvoiceId,
    Guid DiscountId,
    string RejectionReason,
    Guid ManagerId);

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
/// Rejects discount with reason and recalculates invoice totals.
/// </summary>
public static class RejectDiscountHandler
{
    public static async Task<Result> Handle(
        RejectDiscountCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
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

        discount.Reject(command.ManagerId, command.RejectionReason);
        invoice.RecalculateAfterDiscountApproval();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
