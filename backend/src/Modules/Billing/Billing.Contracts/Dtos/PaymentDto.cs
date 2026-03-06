namespace Billing.Contracts.Dtos;

/// <summary>
/// Payment record associated with an invoice.
/// Method is int-serialized Billing.Domain.Enums.PaymentMethod.
/// Status is int-serialized Billing.Domain.Enums.PaymentStatus.
/// </summary>
public sealed record PaymentDto(
    Guid Id,
    Guid InvoiceId,
    int Method,
    decimal Amount,
    int Status,
    string? ReferenceNumber,
    string? CardLast4,
    string? CardType,
    string? Notes,
    Guid RecordedById,
    DateTime RecordedAt,
    Guid? CashierShiftId,
    Guid? TreatmentPackageId,
    bool IsSplitPayment,
    int? SplitSequence);
