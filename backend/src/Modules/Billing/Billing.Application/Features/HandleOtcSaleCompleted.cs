using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.Extensions.Logging;
using Pharmacy.Contracts.IntegrationEvents;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for OtcSaleCompletedIntegrationEvent.
/// Creates a standalone invoice (no VisitId) with per-drug line items.
/// Supports anonymous customers (nullable PatientId).
/// Sends SignalR notification after invoice creation.
/// </summary>
public static class HandleOtcSaleCompletedHandler
{
    public static async Task Handle(
        OtcSaleCompletedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IBillingNotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var number = await invoiceRepository.GetNextInvoiceNumberAsync(DateTime.UtcNow.Year, ct);

        var invoice = Invoice.Create(
            number,
            @event.PatientId,
            @event.CustomerName ?? "Anonymous",
            null, // OTC sales have no visit
            new BranchId(@event.BranchId));

        foreach (var item in @event.Items)
        {
            invoice.AddLineItem(
                item.DrugName,
                item.DrugNameVi,
                item.UnitPrice,
                item.Quantity,
                Department.Pharmacy,
                @event.OtcSaleId,
                "OtcSale");
        }

        invoiceRepository.Add(invoice);
        await unitOfWork.SaveChangesAsync(ct);

        await notificationService.NotifyInvoiceCreatedAsync(
            invoice.Id, invoice.InvoiceNumber, null, @event.CustomerName ?? "Anonymous", invoice.TotalAmount, ct);
    }
}
