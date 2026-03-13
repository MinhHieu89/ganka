using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.Extensions.Logging;
using Optical.Contracts.IntegrationEvents;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for GlassesOrderCreatedIntegrationEvent.
/// Adds frame/lens line items to the visit invoice using get-or-create pattern.
/// VisitId is always present (GlassesOrder entity enforces non-nullable VisitId).
/// Sends SignalR notification after adding line items.
/// </summary>
public static class HandleGlassesOrderCreatedHandler
{
    public static async Task Handle(
        GlassesOrderCreatedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IBillingNotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByVisitIdAsync(@event.VisitId, ct)
            ?? await CreateInvoiceAsync(@event.PatientId, @event.PatientName, @event.VisitId, invoiceRepository, @event.BranchId, ct);

        // Idempotency: skip items already billed from this glasses order
        var existingDescriptions = invoice.LineItems
            .Where(li => li.SourceType == "GlassesOrder" && li.SourceId == @event.OrderId)
            .Select(li => li.Description)
            .ToHashSet();

        foreach (var item in @event.Items)
        {
            if (existingDescriptions.Contains(item.Description))
                continue;

            invoice.AddLineItem(
                item.Description,
                item.DescriptionVi,
                item.UnitPrice,
                item.Quantity,
                Department.Optical,
                @event.OrderId,
                "GlassesOrder");
        }

        await unitOfWork.SaveChangesAsync(ct);

        foreach (var item in @event.Items)
        {
            await notificationService.NotifyLineItemAddedAsync(
                invoice.Id, invoice.InvoiceNumber, item.Description, item.UnitPrice * item.Quantity, "Optical", ct);
        }
    }

    private static async Task<Invoice> CreateInvoiceAsync(
        Guid patientId, string patientName, Guid? visitId,
        IInvoiceRepository invoiceRepository, Guid branchId, CancellationToken ct)
    {
        var number = await invoiceRepository.GetNextInvoiceNumberAsync(DateTime.UtcNow.Year, ct);
        var invoice = Invoice.Create(number, patientId, patientName, visitId, new BranchId(branchId));
        invoiceRepository.Add(invoice);
        return invoice;
    }
}
