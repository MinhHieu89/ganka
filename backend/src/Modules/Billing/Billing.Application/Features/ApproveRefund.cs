using Billing.Application.Interfaces;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to approve a requested refund.
/// </summary>
public sealed record ApproveRefundCommand(
    Guid InvoiceId,
    Guid RefundId,
    Guid ManagerId);

/// <summary>
/// Validator for <see cref="ApproveRefundCommand"/>.
/// </summary>
public class ApproveRefundCommandValidator : AbstractValidator<ApproveRefundCommand>
{
    public ApproveRefundCommandValidator()
    {
        RuleFor(x => x.RefundId).NotEmpty();
    }
}

/// <summary>
/// Wolverine handler for <see cref="ApproveRefundCommand"/>.
/// Validates refund is in Requested status, then approves the refund.
/// </summary>
public static class ApproveRefundHandler
{
    public static async Task<Result> Handle(
        ApproveRefundCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result.Failure(Error.NotFound("Invoice", command.InvoiceId));

        var refund = invoice.Refunds.FirstOrDefault(r => r.Id == command.RefundId);
        if (refund is null)
            return Result.Failure(Error.NotFound("Refund", command.RefundId));

        if (refund.Status != RefundStatus.Requested)
            return Result.Failure(Error.Validation("Only refunds in Requested status can be approved."));

        refund.Approve(command.ManagerId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
