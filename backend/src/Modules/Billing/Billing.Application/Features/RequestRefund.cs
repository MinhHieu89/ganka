using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to request a refund on a finalized invoice.
/// Creates a refund in Requested status pending manager/owner approval.
/// </summary>
public sealed record RequestRefundCommand(
    Guid InvoiceId,
    Guid? InvoiceLineItemId,
    decimal Amount,
    string Reason);

/// <summary>
/// Validator for <see cref="RequestRefundCommand"/>.
/// </summary>
public class RequestRefundCommandValidator : AbstractValidator<RequestRefundCommand>
{
    public RequestRefundCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Refund amount must be greater than zero.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Refund reason is required.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="RequestRefundCommand"/>.
/// Creates a refund request on a finalized invoice.
/// </summary>
public static class RequestRefundHandler
{
    public static async Task<Result<RefundDto>> Handle(
        RequestRefundCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IValidator<RequestRefundCommand> validator,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<RefundDto>.Failure(Error.ValidationWithDetails(errors));
        }

        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<RefundDto>.Failure(Error.NotFound("Invoice", command.InvoiceId));

        // Refunds can only be requested on finalized invoices
        if (invoice.Status != InvoiceStatus.Finalized)
            return Result<RefundDto>.Failure(
                Error.Validation("Refunds can only be requested on finalized invoices."));

        // Validate refund amount does not exceed invoice total
        if (command.Amount > invoice.TotalAmount)
            return Result<RefundDto>.Failure(
                Error.Validation("Refund amount cannot exceed the invoice total amount."));

        // If line item specified, validate it exists and amount does not exceed line total
        if (command.InvoiceLineItemId.HasValue)
        {
            var lineItem = invoice.LineItems.FirstOrDefault(li => li.Id == command.InvoiceLineItemId.Value);
            if (lineItem is null)
                return Result<RefundDto>.Failure(
                    Error.NotFound("InvoiceLineItem", command.InvoiceLineItemId.Value));

            if (command.Amount > lineItem.LineTotal)
                return Result<RefundDto>.Failure(
                    Error.Validation("Refund amount cannot exceed the line item total."));
        }

        // Create refund in Requested status
        var refund = Refund.Create(
            command.InvoiceId,
            command.Amount,
            command.Reason,
            currentUser.UserId,
            command.InvoiceLineItemId);

        // Invoice is tracked via GetByIdAsync -- no Update() needed
        invoice.AddRefund(refund);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new RefundDto(
            refund.Id,
            refund.InvoiceLineItemId,
            refund.Amount,
            refund.Reason,
            (int)refund.Status,
            refund.RequestedById,
            refund.RequestedAt,
            refund.ApprovedById,
            refund.ApprovedAt,
            refund.ProcessedById,
            refund.ProcessedAt);

        return dto;
    }
}
