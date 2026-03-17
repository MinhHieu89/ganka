using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Billing.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time cashier dashboard updates.
/// Authenticated users can join/leave the "cashier-dashboard" group to receive
/// InvoiceCreated, LineItemAdded, LineItemRemoved, and InvoiceVoided notifications.
/// </summary>
[Authorize]
public class BillingHub : Hub
{
    private const string CashierDashboardGroup = "cashier-dashboard";

    public async Task JoinCashierDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, CashierDashboardGroup);
    }

    public async Task LeaveCashierDashboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, CashierDashboardGroup);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, CashierDashboardGroup);
        await base.OnDisconnectedAsync(exception);
    }
}
