using Billing.Application.Interfaces;
using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to approve a pending discount.
/// </summary>
public sealed record ApproveDiscountCommand(
    Guid InvoiceId,
    Guid DiscountId,
    Guid ManagerId);

/// <summary>
/// Wolverine handler for <see cref="ApproveDiscountCommand"/>.
/// Approves discount and recalculates invoice totals.
/// </summary>
public static class ApproveDiscountHandler
{
    public static async Task<Result> Handle(
        ApproveDiscountCommand command,
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

        discount.Approve(command.ManagerId);
        invoice.RecalculateAfterDiscountApproval();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
