using Shared.Domain;

namespace Billing.Domain.Events;

/// <summary>
/// Domain event raised when an invoice is finalized (fully paid and closed).
/// </summary>
public sealed record InvoiceFinalizedEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    decimal TotalAmount) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
