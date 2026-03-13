using Shared.Domain;

namespace Pharmacy.Domain.Events;

/// <summary>
/// Domain event raised when drugs are dispensed against a prescription.
/// Contains per-line drug details and prices for downstream billing integration.
/// </summary>
public sealed record DrugDispensedEvent(
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    List<DrugDispensedEvent.DrugLineDto> Items,
    Guid BranchId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Represents a single drug line item in a dispensing event.
    /// </summary>
    /// <param name="DrugName">English drug name.</param>
    /// <param name="DrugNameVi">Vietnamese drug name.</param>
    /// <param name="Quantity">Quantity dispensed.</param>
    /// <param name="UnitPrice">Selling price per unit at time of dispensing (VND).</param>
    public sealed record DrugLineDto(string DrugName, string DrugNameVi, int Quantity, decimal UnitPrice);
}
