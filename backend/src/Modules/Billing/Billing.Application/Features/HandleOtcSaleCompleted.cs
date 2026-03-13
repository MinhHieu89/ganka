using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Pharmacy.Contracts.IntegrationEvents;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for OtcSaleCompletedIntegrationEvent.
/// Creates a standalone invoice (no VisitId) with per-drug line items.
/// Supports anonymous customers (nullable PatientId).
/// </summary>
public static class HandleOtcSaleCompletedHandler
{
    public static async Task Handle(
        OtcSaleCompletedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var number = await invoiceRepository.GetNextInvoiceNumberAsync(DateTime.UtcNow.Year, ct);

        var invoice = Invoice.Create(
            number,
            @event.PatientId ?? Guid.Empty,
            @event.CustomerName ?? "Anonymous",
            null, // OTC sales have no visit
            new BranchId(currentUser.BranchId));

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
    }
}
