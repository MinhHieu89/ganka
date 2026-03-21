using Clinical.Application.Interfaces;
using Clinical.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Clinical.Infrastructure.Services;

/// <summary>
/// Pushes real-time OSDI submission notifications to the visit SignalR group.
/// All methods are fire-and-forget: failures are logged as warnings but never thrown,
/// ensuring SignalR outages cannot break OSDI submission processing.
/// </summary>
public sealed class OsdiNotificationService(
    IHubContext<OsdiHub> hubContext,
    ILogger<OsdiNotificationService> logger) : IOsdiNotificationService
{
    public async Task NotifyOsdiSubmittedAsync(
        Guid visitId,
        decimal score,
        string severity,
        CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group($"visit-{visitId}")
                .SendAsync("OsdiSubmitted", new
                {
                    VisitId = visitId,
                    Score = score,
                    Severity = severity,
                    SubmittedAt = DateTime.UtcNow
                }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send OsdiSubmitted notification for visit {VisitId}", visitId);
        }
    }

    public async Task NotifyTokenSubmittedAsync(
        string token,
        decimal score,
        string severity,
        CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group($"osdi-token-{token}")
                .SendAsync("OsdiTokenSubmitted", new
                {
                    Token = token,
                    Score = score,
                    Severity = severity,
                    SubmittedAt = DateTime.UtcNow
                }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send OsdiTokenSubmitted notification for token {Token}", token);
        }
    }
}
