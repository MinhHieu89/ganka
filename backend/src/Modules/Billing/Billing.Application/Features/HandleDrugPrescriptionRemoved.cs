using Billing.Application.Interfaces;
using Clinical.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for DrugPrescriptionRemovedIntegrationEvent.
/// Removes prescription-linked line items from the invoice when a doctor
/// removes a drug prescription from a visit.
/// Idempotent: does nothing if no invoice or no matching items found.
/// </summary>
public static class HandleDrugPrescriptionRemovedHandler
{
    public static async Task Handle(
        DrugPrescriptionRemovedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IBillingNotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByVisitIdAsync(@event.VisitId, ct);
        if (invoice is null)
        {
            logger.LogInformation(
                "No invoice found for visit {VisitId} when removing prescription items. Skipping.",
                @event.VisitId);
            return;
        }

        var previousCount = invoice.LineItems.Count;
        invoice.RemoveLineItemsBySource(@event.VisitId, "Prescription");
        var removedCount = previousCount - invoice.LineItems.Count;

        if (removedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(ct);

            await notificationService.NotifyLineItemRemovedAsync(
                invoice.Id, invoice.InvoiceNumber, removedCount, ct);

            logger.LogInformation(
                "Removed {Count} prescription line items from invoice {InvoiceNumber} for visit {VisitId}",
                removedCount, invoice.InvoiceNumber, @event.VisitId);
        }
        else
        {
            logger.LogInformation(
                "No prescription line items found to remove from invoice {InvoiceNumber} for visit {VisitId}",
                invoice.InvoiceNumber, @event.VisitId);
        }
    }
}
