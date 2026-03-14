using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clinical.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time OSDI submission notifications.
/// Authenticated users can join/leave visit groups to receive
/// OsdiSubmitted events when patients submit via the public QR page.
/// </summary>
[Authorize]
public class OsdiHub : Hub
{
    public async Task JoinVisit(string visitId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"visit-{visitId}");
    }

    public async Task LeaveVisit(string visitId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"visit-{visitId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
