using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to record a payment against an invoice.
/// Method is int-serialized PaymentMethod enum.
/// </summary>
public sealed record RecordPaymentCommand(
    Guid InvoiceId,
    int Method,
    decimal Amount,
    string? ReferenceNumber,
    string? CardLast4,
    string? CardType,
    string? Notes,
    Guid? TreatmentPackageId,
    bool IsSplitPayment,
    int? SplitSequence);

/// <summary>
/// Validator for <see cref="RecordPaymentCommand"/>.
/// Validates required fields and method-specific constraints.
/// </summary>
public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty().WithMessage("Invoice is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.Method)
            .InclusiveBetween(0, 6)
            .WithMessage("Invalid payment method.");

        // Card payments require CardLast4 of exactly 4 characters
        RuleFor(x => x.CardLast4)
            .Length(4)
            .When(x => x.Method is (int)PaymentMethod.CardVisa or (int)PaymentMethod.CardMastercard)
            .WithMessage("Card last 4 digits must be exactly 4 characters.");

        // Bank transfer and QR methods require ReferenceNumber
        RuleFor(x => x.ReferenceNumber)
            .NotEmpty()
            .When(x => x.Method is (int)PaymentMethod.BankTransfer
                or (int)PaymentMethod.QrVnPay
                or (int)PaymentMethod.QrMomo
                or (int)PaymentMethod.QrZaloPay)
            .WithMessage("Reference number is required for this payment method.");
    }
}

/// <summary>
/// Wolverine handler for recording a payment against an invoice.
/// Creates a confirmed payment, updates invoice PaidAmount via domain method,
/// and updates cashier shift totals based on payment method.
/// </summary>
public static class RecordPaymentHandler
{
    public static async Task<Result<PaymentDto>> Handle(
        RecordPaymentCommand command,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        ICashierShiftRepository cashierShiftRepository,
        IUnitOfWork unitOfWork,
        IValidator<RecordPaymentCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        // Validate command
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<PaymentDto>.Failure(Error.ValidationWithDetails(errors));
        }

        // Load invoice
        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, ct);
        if (invoice is null)
            return Result<PaymentDto>.Failure(Error.NotFound("Invoice", command.InvoiceId));

        // Reject payments on voided invoices
        if (invoice.Status == InvoiceStatus.Voided)
            return Result<PaymentDto>.Failure(
                Error.Validation("Cannot record payment on a voided invoice."));

        // Validate amount does not exceed balance due
        if (command.Amount > invoice.BalanceDue)
            return Result<PaymentDto>.Failure(
                Error.Validation("Payment exceeds balance due."));

        // Load current open shift
        var branchId = new BranchId(currentUser.BranchId);
        var shift = await cashierShiftRepository.GetCurrentOpenAsync(branchId, ct);
        if (shift is null)
            return Result<PaymentDto>.Failure(
                Error.Validation("No open cashier shift found. Please open a shift before recording payments."));

        // Create payment entity
        var paymentMethod = (PaymentMethod)command.Method;
        var payment = Payment.Create(
            command.InvoiceId,
            paymentMethod,
            command.Amount,
            currentUser.UserId,
            cashierShiftId: shift.Id,
            referenceNumber: command.ReferenceNumber,
            cardLast4: command.CardLast4,
            cardType: command.CardType,
            notes: command.Notes,
            treatmentPackageId: command.TreatmentPackageId,
            isSplitPayment: command.IsSplitPayment,
            splitSequence: command.SplitSequence);

        // Confirm payment immediately (manual confirmation workflow)
        payment.Confirm();

        // Record payment on invoice (domain method updates PaidAmount)
        invoice.RecordPayment(payment);

        // Update shift totals based on payment method
        if (paymentMethod == PaymentMethod.Cash)
        {
            shift.AddCashReceived(command.Amount);
        }
        else
        {
            shift.AddNonCashRevenue(command.Amount);
        }

        shift.IncrementTransactionCount();

        // Persist (invoice is tracked via GetByIdAsync -- no Update() needed)
        paymentRepository.Add(payment);
        cashierShiftRepository.Update(shift);
        await unitOfWork.SaveChangesAsync(ct);

        // Map to DTO
        var dto = new PaymentDto(
            payment.Id,
            payment.InvoiceId,
            (int)payment.Method,
            payment.Amount,
            (int)payment.Status,
            payment.ReferenceNumber,
            payment.CardLast4,
            payment.CardType,
            payment.Notes,
            payment.RecordedById,
            payment.RecordedAt,
            payment.CashierShiftId,
            payment.TreatmentPackageId,
            payment.IsSplitPayment,
            payment.SplitSequence);

        return dto;
    }
}
