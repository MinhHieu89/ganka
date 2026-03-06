using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to process an approved refund, creating the refund transaction.
/// RefundMethod is int-serialized PaymentMethod enum indicating how refund is disbursed.
/// </summary>
public sealed record ProcessRefundCommand(
    Guid InvoiceId,
    Guid RefundId,
    int RefundMethod,
    string? Notes);

/// <summary>
/// Validator for <see cref="ProcessRefundCommand"/>.
/// </summary>
public class ProcessRefundCommandValidator : AbstractValidator<ProcessRefundCommand>
{
    public ProcessRefundCommandValidator()
    {
        RuleFor(x => x.RefundId).NotEmpty();
        RuleFor(x => x.RefundMethod)
            .InclusiveBetween(0, 6)
            .WithMessage("Invalid refund method.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="ProcessRefundCommand"/>.
/// Validates refund is in Approved status, processes the refund,
/// updates shift cash refunds for cash refunds, and marks original payments as refunded.
/// </summary>
public static class ProcessRefundHandler
{
    public static async Task<Result<RefundDto>> Handle(
        ProcessRefundCommand command,
        IInvoiceRepository invoiceRepository,
        ICashierShiftRepository cashierShiftRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<RefundDto>.Failure(Error.NotFound("Invoice", command.InvoiceId));

        var refund = invoice.Refunds.FirstOrDefault(r => r.Id == command.RefundId);
        if (refund is null)
            return Result<RefundDto>.Failure(Error.NotFound("Refund", command.RefundId));

        // Validate refund status is Approved
        if (refund.Status != RefundStatus.Approved)
            return Result<RefundDto>.Failure(
                Error.Validation("Only approved refunds can be processed."));

        // Process the refund (domain method validates Approved status)
        refund.Process(currentUser.UserId);

        var refundMethod = (PaymentMethod)command.RefundMethod;

        // If refund method is Cash, update the current open shift's cash refunds
        if (refundMethod == PaymentMethod.Cash)
        {
            var branchId = new BranchId(currentUser.BranchId);
            var shift = await cashierShiftRepository.GetCurrentOpenAsync(branchId, cancellationToken);
            if (shift is null)
                return Result<RefundDto>.Failure(
                    Error.Validation("No open cashier shift found. Please open a shift before processing cash refunds."));

            shift.AddCashRefund(refund.Amount);
            cashierShiftRepository.Update(shift);
        }

        // Mark original confirmed payments as refunded if applicable
        var confirmedPayments = invoice.Payments
            .Where(p => p.Status == PaymentStatus.Confirmed)
            .ToList();

        foreach (var payment in confirmedPayments)
        {
            payment.MarkRefunded();
            paymentRepository.Update(payment);
        }

        invoiceRepository.Update(invoice);
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
