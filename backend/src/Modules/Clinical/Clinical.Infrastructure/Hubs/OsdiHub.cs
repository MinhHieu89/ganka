using Clinical.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clinical.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time OSDI submission notifications.
/// Authenticated users can join/leave visit groups to receive
/// OsdiSubmitted events when patients submit via the public QR page.
/// Validates visitId as Guid and checks user authorization via visit repository.
/// </summary>
[Authorize]
public class OsdiHub : Hub
{
    private readonly IVisitRepository _visitRepository;

    public OsdiHub(IVisitRepository visitRepository)
    {
        _visitRepository = visitRepository;
    }

    public async Task JoinVisit(Guid visitId)
    {
        // Verify the visit exists and the authenticated user has access
        var visit = await _visitRepository.GetByIdAsync(visitId);
        if (visit is null)
        {
            throw new HubException("Visit not found.");
        }

        // Check that the authenticated user is the doctor assigned to this visit
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId) || visit.DoctorId.ToString() != userId)
        {
            throw new HubException("You are not authorized to access this visit.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"visit-{visitId}");
    }

    public async Task LeaveVisit(Guid visitId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"visit-{visitId}");
    }

    /// <summary>
    /// Joins a token-scoped group for receiving OSDI submission notifications.
    /// Used by treatment session flow where VisitId is null.
    /// </summary>
    public async Task JoinToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new HubException("Token is required.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"osdi-token-{token}");
    }

    public async Task LeaveToken(string token)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"osdi-token-{token}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
