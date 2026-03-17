using Shared.Domain;

namespace Clinical.Domain.Events;

/// <summary>
/// Domain event raised when a drug prescription is added to a visit.
/// Used internally within the Clinical module. The Clinical.Application layer
/// converts this to an integration event for cross-module communication (billing).
/// Items carry DrugCatalogItemId for price lookup by the Billing module.
/// </summary>
public sealed record DrugPrescriptionAddedEvent(
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    Guid BranchId,
    List<DrugPrescriptionAddedEvent.PrescribedDrugDto> Items) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Represents a single prescribed drug in the domain event.
    /// DrugCatalogItemId is nullable for off-catalog drugs.
    /// </summary>
    public sealed record PrescribedDrugDto(
        string DrugName,
        Guid? DrugCatalogItemId,
        int Quantity);
}
