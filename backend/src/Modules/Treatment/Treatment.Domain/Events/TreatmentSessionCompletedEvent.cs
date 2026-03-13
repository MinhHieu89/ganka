using Shared.Domain;
using Treatment.Domain.Enums;
using Treatment.Domain.Models;

namespace Treatment.Domain.Events;

/// <summary>
/// Domain event raised when a treatment session is completed.
/// Used internally within the Treatment module. The Treatment.Application layer
/// converts this to an integration event for cross-module communication.
/// </summary>
public sealed record TreatmentSessionCompletedEvent(
    Guid PackageId,
    Guid SessionId,
    Guid PatientId,
    TreatmentType TreatmentType,
    List<ConsumableUsageInfo> Consumables,
    Guid? VisitId,
    decimal SessionFeeAmount,
    Guid BranchId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
