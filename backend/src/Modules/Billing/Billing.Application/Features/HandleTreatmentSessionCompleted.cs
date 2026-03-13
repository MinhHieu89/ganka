using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared.Domain;
using Treatment.Contracts.IntegrationEvents;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for TreatmentSessionCompletedIntegrationEvent.
/// Uses event.VisitId and event.SessionFeeAmount directly to add a session fee line item.
/// Skips billing if VisitId is null (treatment not linked to a visit).
/// Sends SignalR notification after adding line item.
/// </summary>
public static class HandleTreatmentSessionCompletedHandler
{
    private static readonly Dictionary<int, (string En, string Vi)> TreatmentTypeNames = new()
    {
        { 0, ("IPL Treatment Session", "Buoi dieu tri IPL") },
        { 1, ("LLLT Treatment Session", "Buoi dieu tri LLLT") },
        { 2, ("LidCare Treatment Session", "Buoi dieu tri LidCare") }
    };

    public static async Task Handle(
        TreatmentSessionCompletedIntegrationEvent @event,
        IInvoiceRepository invoiceRepository,
        IBillingNotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        if (@event.VisitId is null)
        {
            logger.LogWarning(
                "TreatmentSessionCompleted for Session {SessionId} has no VisitId. Skipping billing.",
                @event.SessionId);
            return;
        }

        var visitId = @event.VisitId.Value;
        var invoice = await invoiceRepository.GetByVisitIdAsync(visitId, ct);
        if (invoice is null)
        {
            var number = await invoiceRepository.GetNextInvoiceNumberAsync(DateTime.UtcNow.Year, ct);
            invoice = Invoice.Create(
                number,
                @event.PatientId,
                @event.PatientName,
                visitId,
                new BranchId(@event.BranchId));
            invoiceRepository.Add(invoice);
        }

        var (descriptionEn, descriptionVi) = TreatmentTypeNames.GetValueOrDefault(
            @event.TreatmentType, ("Treatment Session", "Buoi dieu tri"));

        invoice.AddLineItem(
            descriptionEn,
            descriptionVi,
            @event.SessionFeeAmount,
            1,
            Department.Treatment,
            @event.SessionId,
            "TreatmentSession");

        await unitOfWork.SaveChangesAsync(ct);

        try
        {
            await notificationService.NotifyLineItemAddedAsync(
                invoice.Id, invoice.InvoiceNumber, descriptionEn, @event.SessionFeeAmount, "Treatment", ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send SignalR notification for treatment session on invoice {InvoiceId}", invoice.Id);
        }
    }
}
