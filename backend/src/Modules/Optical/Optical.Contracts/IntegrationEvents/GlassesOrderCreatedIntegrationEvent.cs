namespace Optical.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when a glasses order is created.
/// Consumed by the Billing module to add frame/lens line items
/// to the visit invoice (INT-05).
/// </summary>
public sealed record GlassesOrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid? VisitId,
    Guid PatientId,
    string PatientName,
    List<GlassesOrderCreatedIntegrationEvent.OrderLineDto> Items,
    Guid BranchId)
{
    /// <summary>
    /// Represents a glasses order line item for cross-module consumption.
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
