using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Pharmacy.Contracts.IntegrationEvents;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for DrugDispensedIntegrationEvent.
/// Adds per-drug line items to the visit invoice using get-or-create pattern.
/// </summary>
public static class HandleDrugDispensedHandler
{
    public static async Task Handle(
        DrugDispensedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
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
                new BranchId(currentUser.BranchId));
            invoiceRepository.Add(invoice);
        }

        foreach (var item in @event.Items)
        {
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
    }
}
