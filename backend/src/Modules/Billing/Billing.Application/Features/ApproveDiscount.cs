using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Shared.Domain;
using Wolverine;

namespace Billing.Application.Features;

/// <summary>
/// Command to approve a pending discount with manager PIN verification.
/// </summary>
public sealed record ApproveDiscountCommand(
    Guid InvoiceId,
    Guid DiscountId,
    Guid ManagerId,
    string ManagerPin);

/// <summary>
/// Cross-module query to verify a manager's PIN.
/// Sent to Auth module via IMessageBus.
/// </summary>
public sealed record VerifyManagerPinQuery(Guid ManagerId, string Pin);

/// <summary>
/// Response from Auth module for PIN verification.
/// </summary>
public sealed record VerifyManagerPinResponse(bool IsValid);

/// <summary>
/// Wolverine handler for <see cref="ApproveDiscountCommand"/>.
/// Verifies manager PIN via cross-module query, then approves discount and recalculates invoice totals.
/// </summary>
public static class ApproveDiscountHandler
{
    public static async Task<Result> Handle(
        ApproveDiscountCommand command,
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

        // Approve the discount and recalculate invoice totals
        discount.Approve(command.ManagerId);
        invoice.RecalculateAfterDiscountApproval();

        invoiceRepository.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
