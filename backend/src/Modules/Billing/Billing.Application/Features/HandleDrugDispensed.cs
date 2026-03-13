using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.Extensions.Logging;
using Pharmacy.Contracts.IntegrationEvents;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for DrugDispensedIntegrationEvent.
/// Adds per-drug line items to the visit invoice using get-or-create pattern.
/// Sends SignalR notification after adding line items.
/// </summary>
public static class HandleDrugDispensedHandler
{
    public static async Task Handle(
        DrugDispensedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IBillingNotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByVisitIdAsync(@event.VisitId, ct);
        if (invoice is null)
        {
            var number = await invoiceRepository.GetNextInvoiceNumberAsync(DateTime.UtcNow.Year, ct);
            invoice = Invoice.Create(
                number,
                @event.PatientId,
                @event.PatientName,
                @event.VisitId,
                new BranchId(@event.BranchId));
            invoiceRepository.Add(invoice);
        }

        // Idempotency: skip items already billed from this dispensing event
        var existingDescriptions = invoice.LineItems
            .Where(li => li.SourceType == "Dispensing" && li.SourceId == @event.VisitId)
            .Select(li => li.Description)
            .ToHashSet();

        foreach (var item in @event.Items)
        {
            if (item.UnitPrice <= 0)
            {
                logger.LogWarning("Skipping zero-price dispensing item {DrugName} on invoice {InvoiceId}", item.DrugName, invoice.Id);
                continue;
            }

            if (existingDescriptions.Contains(item.DrugName))
                continue;

            invoice.AddLineItem(
                item.DrugName,
                item.DrugNameVi,
                item.UnitPrice,
                item.Quantity,
                Department.Pharmacy,
                @event.VisitId,
                "Dispensing");
        }

        await unitOfWork.SaveChangesAsync(ct);

        try
        {
            foreach (var item in @event.Items)
            {
                await notificationService.NotifyLineItemAddedAsync(
                    invoice.Id, invoice.InvoiceNumber, item.DrugName, item.UnitPrice * item.Quantity, "Pharmacy", ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send SignalR notification for drug dispensed on invoice {InvoiceId}", invoice.Id);
        }
    }
}
