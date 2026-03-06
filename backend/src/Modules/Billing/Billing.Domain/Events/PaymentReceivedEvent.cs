using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Events;

/// <summary>
/// Published when a payment is confirmed against an invoice.
/// Used by CashierShift handlers to update shift revenue totals.
/// </summary>
public sealed record PaymentReceivedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid InvoiceId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public PaymentMethod Method { get; init; }
}
