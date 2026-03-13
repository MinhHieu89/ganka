namespace Pharmacy.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when an OTC sale is completed.
/// Consumed by the Billing module to create invoice line items for OTC sales.
/// </summary>
public sealed record OtcSaleCompletedIntegrationEvent(
    Guid OtcSaleId,
    Guid? PatientId,
    string? CustomerName,
    List<OtcSaleCompletedIntegrationEvent.DrugLineDto> Items)
{
    /// <summary>
    /// Represents a single drug line item in an OTC sale.
    /// </summary>
    /// <param name="DrugName">English drug name.</param>
    /// <param name="DrugNameVi">Vietnamese drug name.</param>
    /// <param name="Quantity">Quantity sold.</param>
    /// <param name="UnitPrice">Selling price per unit at time of sale (VND).</param>
    public sealed record DrugLineDto(string DrugName, string DrugNameVi, int Quantity, decimal UnitPrice);
}
