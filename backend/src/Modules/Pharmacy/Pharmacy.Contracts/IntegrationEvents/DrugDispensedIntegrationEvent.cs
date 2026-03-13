namespace Pharmacy.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when drugs are dispensed against a prescription.
/// Consumed by the Billing module to create invoice line items for dispensed drugs.
/// </summary>
public sealed record DrugDispensedIntegrationEvent(
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    List<DrugDispensedIntegrationEvent.DrugLineDto> Items,
    Guid BranchId)
{
    /// <summary>
    /// Represents a single drug line item in a dispensing event.
    /// </summary>
    /// <param name="DrugName">English drug name.</param>
    /// <param name="DrugNameVi">Vietnamese drug name.</param>
    /// <param name="Quantity">Quantity dispensed.</param>
    /// <param name="UnitPrice">Selling price per unit at time of dispensing (VND).</param>
    public sealed record DrugLineDto(string DrugName, string DrugNameVi, int Quantity, decimal UnitPrice);
}
