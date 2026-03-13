using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Clinical.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for VisitCreatedIntegrationEvent.
/// Creates a Draft invoice and adds a consultation fee line item from the service catalog.
/// </summary>
public static class HandleVisitCreatedHandler
{
    public static async Task Handle(
        VisitCreatedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IServiceCatalogRepository serviceCatalogRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger logger,
        CancellationToken ct)
    {
        var invoiceNumber = await invoiceRepository.GetNextInvoiceNumberAsync(DateTime.UtcNow.Year, ct);

        var invoice = Invoice.Create(
            invoiceNumber,
            @event.PatientId,
            @event.PatientName,
            @event.VisitId,
            new BranchId(currentUser.BranchId));

        var consultation = await serviceCatalogRepository.GetActiveByCodeAsync("CONSULTATION", ct);
        if (consultation is not null)
        {
            invoice.AddLineItem(
                consultation.Name,
                consultation.NameVi,
                consultation.Price,
                1,
                Billing.Domain.Enums.Department.Medical,
                @event.VisitId,
                "Visit");
        }
        else
        {
            logger.LogWarning("CONSULTATION service not found in catalog. Invoice {InvoiceNumber} created without consultation fee for Visit {VisitId}.",
                invoiceNumber, @event.VisitId);
        }

        invoiceRepository.Add(invoice);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
