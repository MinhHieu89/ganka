using Shared.Domain;

namespace Billing.Domain.Events;

/// <summary>
/// Published when an invoice is finalized (fully paid and locked).
/// </summary>
public sealed record InvoiceFinalizedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = default!;
    public decimal TotalAmount { get; init; }
}
