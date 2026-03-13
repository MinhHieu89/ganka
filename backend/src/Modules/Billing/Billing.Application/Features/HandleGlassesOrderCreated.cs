using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Optical.Contracts.IntegrationEvents;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for GlassesOrderCreatedIntegrationEvent.
/// Adds frame/lens line items to the visit invoice using get-or-create pattern.
/// Supports standalone orders (nullable VisitId).
/// </summary>
public static class HandleGlassesOrderCreatedHandler
{
    public static async Task Handle(
        GlassesOrderCreatedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
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
