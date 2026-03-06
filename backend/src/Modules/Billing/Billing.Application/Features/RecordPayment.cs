using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to record a payment against an invoice.
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
/// </summary>
public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty().WithMessage("Invoice is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}

/// <summary>
/// Wolverine handler for recording a payment against an invoice.
/// Stub: returns failure until implemented.
/// </summary>
public static class RecordPaymentHandler
{
    public static Task<Result<PaymentDto>> Handle(
        RecordPaymentCommand command,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        ICashierShiftRepository cashierShiftRepository,
        IUnitOfWork unitOfWork,
        IValidator<RecordPaymentCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        // Stub: not implemented yet (TDD RED phase)
        return Task.FromResult(Result<PaymentDto>.Failure(
            Error.Custom("Error.NotImplemented", "RecordPayment handler not yet implemented.")));
    }
}
