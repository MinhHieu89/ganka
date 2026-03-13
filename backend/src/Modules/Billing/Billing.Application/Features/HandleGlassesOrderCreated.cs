using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.Extensions.Logging;
using Optical.Contracts.IntegrationEvents;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for GlassesOrderCreatedIntegrationEvent.
/// Adds frame/lens line items to the visit invoice using get-or-create pattern.
/// Supports standalone orders (nullable VisitId).
/// Sends SignalR notification after adding line items.
/// </summary>
public static class HandleGlassesOrderCreatedHandler
{
    public static async Task Handle(
        GlassesOrderCreatedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IBillingNotificationService notificationService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger logger,
        CancellationToken ct)
    {
        Invoice invoice;

        if (@event.VisitId.HasValue)
        {
            invoice = await invoiceRepository.GetByVisitIdAsync(@event.VisitId.Value, ct)
                ?? await CreateInvoiceAsync(@event.PatientId, @event.PatientName, @event.VisitId, invoiceRepository, currentUser, ct);
        }
        else
        {
            invoice = await CreateInvoiceAsync(@event.PatientId, @event.PatientName, null, invoiceRepository, currentUser, ct);
        }

        foreach (var item in @event.Items)
        {
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

        try
        {
            foreach (var item in @event.Items)
            {
                await notificationService.NotifyLineItemAddedAsync(
                    invoice.Id, invoice.InvoiceNumber, item.Description, item.UnitPrice * item.Quantity, "Optical", ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send SignalR notification for glasses order on invoice {InvoiceId}", invoice.Id);
        }
    }

    private static async Task<Invoice> CreateInvoiceAsync(
        Guid patientId, string patientName, Guid? visitId,
        IInvoiceRepository invoiceRepository, ICurrentUser currentUser, CancellationToken ct)
    {
        var number = await invoiceRepository.GetNextInvoiceNumberAsync(DateTime.UtcNow.Year, ct);
        var invoice = Invoice.Create(number, patientId, patientName, visitId, new BranchId(currentUser.BranchId));
        invoiceRepository.Add(invoice);
        return invoice;
    }
}
