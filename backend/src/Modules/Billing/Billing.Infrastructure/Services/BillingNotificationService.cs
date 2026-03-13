using Billing.Application.Interfaces;
using Billing.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Billing.Infrastructure.Services;

/// <summary>
/// Pushes real-time billing notifications to the "cashier-dashboard" SignalR group.
/// All methods are fire-and-forget: failures are logged as warnings but never thrown,
/// ensuring SignalR outages cannot break invoice processing pipelines.
/// </summary>
public sealed class BillingNotificationService(
    IHubContext<BillingHub> hubContext,
    ILogger<BillingNotificationService> logger) : IBillingNotificationService
{
    private const string CashierDashboardGroup = "cashier-dashboard";

    public async Task NotifyInvoiceCreatedAsync(
        Guid invoiceId,
        string invoiceNumber,
        Guid? visitId,
        string patientName,
        decimal totalAmount,
        CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group(CashierDashboardGroup)
                .SendAsync("InvoiceCreated", new
                {
                    InvoiceId = invoiceId,
                    InvoiceNumber = invoiceNumber,
                    VisitId = visitId,
                    PatientName = patientName,
                    TotalAmount = totalAmount
                }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send InvoiceCreated notification for invoice {InvoiceId}", invoiceId);
        }
    }

    public async Task NotifyLineItemAddedAsync(
        Guid invoiceId,
        string invoiceNumber,
        string description,
        decimal amount,
        string department,
        CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group(CashierDashboardGroup)
                .SendAsync("LineItemAdded", new
                {
                    InvoiceId = invoiceId,
                    InvoiceNumber = invoiceNumber,
                    Description = description,
                    Amount = amount,
                    Department = department
                }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send LineItemAdded notification for invoice {InvoiceId}", invoiceId);
        }
    }

    public async Task NotifyInvoiceVoidedAsync(
        Guid invoiceId,
        string invoiceNumber,
        CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group(CashierDashboardGroup)
                .SendAsync("InvoiceVoided", new
                {
                    InvoiceId = invoiceId,
                    InvoiceNumber = invoiceNumber
                }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send InvoiceVoided notification for invoice {InvoiceId}", invoiceId);
        }
    }
}
