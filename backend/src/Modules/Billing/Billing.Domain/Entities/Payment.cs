using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Records a payment transaction against an invoice. Supports multiple payment methods
/// (cash, bank transfer, QR wallets, card) with manual confirmation workflow.
/// Also handles treatment package split payments (50/50 rule per FIN-05).
/// Implements IAuditable for field-level change tracking via AuditInterceptor.
/// </summary>
public class Payment : Entity, IAuditable
{
    /// <summary>Foreign key to the invoice this payment is applied to.</summary>
    public Guid InvoiceId { get; private set; }

    /// <summary>Payment method used (cash, bank transfer, QR, card).</summary>
    public PaymentMethod Method { get; private set; }

    /// <summary>Payment amount in VND.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Current status of this payment.</summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>Bank transfer reference number or QR transaction ID (nullable).</summary>
    public string? ReferenceNumber { get; private set; }

    /// <summary>Last 4 digits of card number for card payments (nullable).</summary>
    public string? CardLast4 { get; private set; }

    /// <summary>Card brand/type string for display (e.g. "Visa", "Mastercard") (nullable).</summary>
    public string? CardType { get; private set; }

    /// <summary>Optional notes about this payment (e.g. manual confirmation details).</summary>
    public string? Notes { get; private set; }

    /// <summary>Foreign key to the user (cashier) who recorded this payment.</summary>
    public Guid RecordedById { get; private set; }

    /// <summary>UTC timestamp when this payment was recorded.</summary>
    public DateTime RecordedAt { get; private set; }

    /// <summary>Foreign key to the cashier shift for reconciliation (nullable).</summary>
    public Guid? CashierShiftId { get; private set; }

    /// <summary>Foreign key to treatment package for split payment tracking (nullable, FIN-05).</summary>
    public Guid? TreatmentPackageId { get; private set; }

    /// <summary>Whether this is a split payment for a treatment package (FIN-05).</summary>
    public bool IsSplitPayment { get; private set; }

    /// <summary>Split sequence number: 1 = first half, 2 = second half (nullable).</summary>
    public int? SplitSequence { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private Payment() { }

    /// <summary>
    /// Factory method for creating a new payment record.
    /// Status defaults to Pending — call Confirm() after payment is verified.
    /// </summary>
    public static Payment Create(
        Guid invoiceId,
        PaymentMethod method,
        decimal amount,
        Guid recordedById,
        string? referenceNumber = null,
        string? cardLast4 = null,
        string? cardType = null,
        string? notes = null,
        Guid? cashierShiftId = null,
        Guid? treatmentPackageId = null,
        bool isSplitPayment = false,
        int? splitSequence = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive.", nameof(amount));

        if (isSplitPayment && splitSequence is not (1 or 2))
            throw new ArgumentException(
                "Split sequence must be 1 or 2 for split payments.", nameof(splitSequence));

        return new Payment
        {
            InvoiceId = invoiceId,
            Method = method,
            Amount = amount,
            Status = PaymentStatus.Pending,
            ReferenceNumber = referenceNumber,
            CardLast4 = cardLast4,
            CardType = cardType,
            Notes = notes,
            RecordedById = recordedById,
            RecordedAt = DateTime.UtcNow,
            CashierShiftId = cashierShiftId,
            TreatmentPackageId = treatmentPackageId,
            IsSplitPayment = isSplitPayment,
            SplitSequence = splitSequence
        };
    }

    /// <summary>
    /// Confirms the payment has been received and verified.
    /// </summary>
    public void Confirm()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot confirm payment in '{Status}' status. Only Pending payments can be confirmed.");

        Status = PaymentStatus.Confirmed;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks this payment as refunded after a refund has been processed.
    /// </summary>
    public void MarkRefunded()
    {
        if (Status != PaymentStatus.Confirmed)
            throw new InvalidOperationException(
                $"Cannot mark payment as refunded in '{Status}' status. Only Confirmed payments can be refunded.");

        Status = PaymentStatus.Refunded;
        SetUpdatedAt();
    }
}
