using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Events;

/// <summary>
/// Domain event raised when a payment is confirmed.
/// </summary>
public sealed record PaymentReceivedEvent(
    Guid InvoiceId,
    Guid PaymentId,
    decimal Amount,
    PaymentMethod Method) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
