using Shared.Domain;

namespace Pharmacy.Domain.Events;

/// <summary>
/// Domain event raised when an OTC (over-the-counter) sale is completed.
/// Contains sale items and optional customer info for downstream billing integration.
/// </summary>
public sealed record OtcSaleCompletedEvent(
    Guid OtcSaleId,
    Guid? PatientId,
    string? CustomerName,
    List<OtcSaleCompletedEvent.DrugLineDto> Items,
    Guid BranchId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Represents a single drug line item in an OTC sale.
    /// </summary>
    /// <param name="DrugName">English drug name.</param>
    /// <param name="DrugNameVi">Vietnamese drug name.</param>
    /// <param name="Quantity">Quantity sold.</param>
    /// <param name="UnitPrice">Selling price per unit at time of sale (VND).</param>
    public sealed record DrugLineDto(string DrugName, string DrugNameVi, int Quantity, decimal UnitPrice);
}
