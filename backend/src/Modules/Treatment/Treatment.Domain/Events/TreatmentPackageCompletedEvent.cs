using Shared.Domain;
using Treatment.Domain.Enums;

namespace Treatment.Domain.Events;

/// <summary>
/// Domain event raised when all sessions in a treatment package have been completed.
/// Used internally for auto-completion workflows and notifications.
/// </summary>
public sealed record TreatmentPackageCompletedEvent(
    Guid PackageId,
    Guid PatientId,
    TreatmentType TreatmentType) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
