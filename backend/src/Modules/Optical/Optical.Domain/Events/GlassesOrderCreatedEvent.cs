using Shared.Domain;

namespace Optical.Domain.Events;

/// <summary>
/// Domain event raised when a new glasses order is created.
/// Carries order metadata and line items (frame/lens) for downstream
/// integration event publishing to the Billing module.
/// </summary>
public sealed record GlassesOrderCreatedEvent(
    Guid OrderId,
    Guid? VisitId,
    Guid PatientId,
    string PatientName,
    List<GlassesOrderCreatedEvent.OrderLineDto> Items,
    Guid BranchId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Represents a line item on the glasses order for event transport.
    /// </summary>
    /// <param name="Description">English item description.</param>
    /// <param name="DescriptionVi">Vietnamese item description.</param>
    /// <param name="UnitPrice">Unit price in VND.</param>
    /// <param name="Quantity">Number of units.</param>
    public sealed record OrderLineDto(
        string Description,
        string DescriptionVi,
        decimal UnitPrice,
        int Quantity);
}
