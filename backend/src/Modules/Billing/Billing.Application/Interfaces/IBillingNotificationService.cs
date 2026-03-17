namespace Billing.Application.Interfaces;

/// <summary>
/// Pushes real-time billing updates to connected cashier dashboards via SignalR.
/// Implementations must be fire-and-forget: failures are logged but never thrown,
/// so SignalR outages cannot break invoice processing.
/// </summary>
public interface IBillingNotificationService
{
    Task NotifyInvoiceCreatedAsync(
        Guid invoiceId,
        string invoiceNumber,
        Guid? visitId,
        string patientName,
        decimal totalAmount,
        CancellationToken ct);

    Task NotifyLineItemAddedAsync(
        Guid invoiceId,
        string invoiceNumber,
        string description,
        decimal amount,
        string department,
        CancellationToken ct);

    Task NotifyLineItemRemovedAsync(
        Guid invoiceId,
        string invoiceNumber,
        int removedCount,
        CancellationToken ct);

    Task NotifyInvoiceVoidedAsync(
        Guid invoiceId,
        string invoiceNumber,
        CancellationToken ct);
}
