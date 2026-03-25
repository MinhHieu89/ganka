using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Payment recorded at Cashier for a visit.
/// Supports combined payment (Visit + Glasses) in the single-invoice model.
/// </summary>
public class VisitPayment : Entity
{
    public Guid VisitId { get; private set; }
    public PaymentType PaymentKind { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public decimal AmountReceived { get; private set; }
    public decimal ChangeGiven { get; private set; }
    public Guid CashierId { get; private set; }
    public string CashierName { get; private set; } = string.Empty;
    public DateTime PaidAt { get; private set; }

    private VisitPayment() { }

    public static VisitPayment Create(Guid visitId, PaymentType paymentKind, decimal amount,
        PaymentMethod method, decimal amountReceived, decimal changeGiven,
        Guid cashierId, string cashierName)
    {
        return new VisitPayment
        {
            VisitId = visitId,
            PaymentKind = paymentKind,
            Amount = amount,
            Method = method,
            AmountReceived = amountReceived,
            ChangeGiven = changeGiven,
            CashierId = cashierId,
            CashierName = cashierName,
            PaidAt = DateTime.UtcNow
        };
    }
}
