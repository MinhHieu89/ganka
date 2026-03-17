using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Clinical.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;
using Wolverine;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for DrugPrescriptionAddedIntegrationEvent.
/// Creates pharmacy line items when a prescription is written (before dispensing),
/// enabling the Vietnamese clinic flow: prescribe -> pay -> dispense.
/// Uses get-or-create invoice pattern and looks up drug prices from Pharmacy via IMessageBus.
/// </summary>
public static class HandleDrugPrescriptionAddedHandler
{
    public static async Task Handle(
        DrugPrescriptionAddedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IMessageBus messageBus,
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

        // Look up prices for catalog-linked items via Pharmacy module
        var catalogItemIds = @event.Items
            .Where(i => i.DrugCatalogItemId.HasValue)
            .Select(i => i.DrugCatalogItemId!.Value)
            .Distinct()
            .ToList();

        var priceLookup = new Dictionary<Guid, DrugCatalogPriceDto>();
        if (catalogItemIds.Count > 0)
        {
            try
            {
                var prices = await messageBus.InvokeAsync<List<DrugCatalogPriceDto>>(
                    new GetDrugCatalogPricesQuery(catalogItemIds), ct);
                priceLookup = prices.ToDictionary(p => p.CatalogItemId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to look up drug catalog prices for visit {VisitId}. Using zero prices.", @event.VisitId);
            }
        }

        // Idempotency: skip items already billed from prescriptions
        var existingPrescriptionItems = invoice.LineItems
            .Where(li => li.SourceType == "Prescription" && li.SourceId == @event.VisitId)
            .Select(li => li.Description)
            .ToHashSet();

        var addedItems = new List<(string DrugName, decimal UnitPrice, int Quantity)>();

        foreach (var item in @event.Items)
        {
            if (existingPrescriptionItems.Contains(item.DrugName))
                continue;

            // Look up price from catalog; use 0 for off-catalog or unknown items
            decimal unitPrice = 0m;
            string? nameVi = null;
            if (item.DrugCatalogItemId.HasValue &&
                priceLookup.TryGetValue(item.DrugCatalogItemId.Value, out var priceDto))
            {
                unitPrice = priceDto.SellingPrice;
                nameVi = priceDto.NameVi;
            }

            invoice.AddLineItem(
                item.DrugName,
                nameVi,
                unitPrice,
                item.Quantity,
                Department.Pharmacy,
                @event.VisitId,
                "Prescription");

            addedItems.Add((item.DrugName, unitPrice, item.Quantity));
        }

        if (addedItems.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(ct);

            foreach (var added in addedItems)
            {
                await notificationService.NotifyLineItemAddedAsync(
                    invoice.Id, invoice.InvoiceNumber, added.DrugName,
                    added.UnitPrice * added.Quantity, "Pharmacy", ct);
            }

            logger.LogInformation(
                "Added {Count} prescription line items to invoice {InvoiceNumber} for visit {VisitId}",
                addedItems.Count, invoice.InvoiceNumber, @event.VisitId);
        }
        else
        {
            logger.LogInformation(
                "No new prescription line items to add for visit {VisitId} (already billed)",
                @event.VisitId);
        }
    }
}
