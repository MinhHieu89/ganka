using Billing.Application.Interfaces;
using Billing.Domain.Enums;
using Clinical.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for VisitCancelledIntegrationEvent.
/// Voids the associated invoice if it exists and is still in Draft status.
/// Idempotent: no-op if invoice is missing or already voided.
/// Sends SignalR notification after voiding.
/// </summary>
public static class HandleVisitCancelledHandler
{
    public static async Task Handle(
        VisitCancelledIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IBillingNotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByVisitIdAsync(@event.VisitId, ct);

        if (invoice is null || invoice.Status != InvoiceStatus.Draft)
            return;

        invoice.Void();
        await unitOfWork.SaveChangesAsync(ct);

        try
        {
            await notificationService.NotifyInvoiceVoidedAsync(invoice.Id, invoice.InvoiceNumber, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send SignalR notification for voided invoice {InvoiceId}", invoice.Id);
        }
    }
}
