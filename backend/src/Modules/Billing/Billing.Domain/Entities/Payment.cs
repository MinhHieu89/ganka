using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Payment entity representing a financial transaction against an invoice.
/// Supports multiple payment methods and treatment package split payments.
/// </summary>
public class Payment : Entity, IAuditable
{
    public Guid InvoiceId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? CardLast4 { get; private set; }
    public string? CardType { get; private set; }
    public string? Notes { get; private set; }
    public Guid RecordedById { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public Guid? CashierShiftId { get; private set; }
    public Guid? TreatmentPackageId { get; private set; }
    public bool IsSplitPayment { get; private set; }
    public int? SplitSequence { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid invoiceId,
        PaymentMethod method,
        decimal amount,
        Guid recordedById,
        Guid? cashierShiftId = null,
        string? referenceNumber = null,
        string? cardLast4 = null,
        string? cardType = null,
        string? notes = null,
        Guid? treatmentPackageId = null,
        bool isSplitPayment = false,
        int? splitSequence = null)
    {
        return new Payment
        {
            InvoiceId = invoiceId,
            Method = method,
            Amount = amount,
            Status = PaymentStatus.Pending,
            RecordedById = recordedById,
            RecordedAt = DateTime.UtcNow,
            CashierShiftId = cashierShiftId,
            ReferenceNumber = referenceNumber,
            CardLast4 = cardLast4,
            CardType = cardType,
            Notes = notes,
            TreatmentPackageId = treatmentPackageId,
            IsSplitPayment = isSplitPayment,
            SplitSequence = splitSequence
        };
    }

    public void Confirm()
    {
        Status = PaymentStatus.Confirmed;
        SetUpdatedAt();
    }

    public void MarkRefunded()
    {
        Status = PaymentStatus.Refunded;
        SetUpdatedAt();
    }
}
