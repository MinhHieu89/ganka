using Billing.Application.Interfaces;
using Billing.Domain.Enums;
using Clinical.Contracts.IntegrationEvents;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for VisitCancelledIntegrationEvent.
/// Voids the associated invoice if it exists and is still in Draft status.
/// Idempotent: no-op if invoice is missing or already voided.
/// </summary>
public static class HandleVisitCancelledHandler
{
    public static async Task Handle(
        VisitCancelledIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByVisitIdAsync(@event.VisitId, ct);

        if (invoice is null || invoice.Status == InvoiceStatus.Voided)
            return;

        invoice.Void();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
