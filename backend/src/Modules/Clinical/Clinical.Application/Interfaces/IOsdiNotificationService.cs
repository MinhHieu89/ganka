namespace Clinical.Application.Interfaces;

/// <summary>
/// Pushes real-time OSDI submission notifications to connected clients via SignalR.
/// Implementations must be fire-and-forget: failures are logged but never thrown,
/// so SignalR outages cannot break OSDI submission processing.
/// </summary>
public interface IOsdiNotificationService
{
    /// <summary>
    /// Notifies the visit group that an OSDI questionnaire was submitted.
    /// </summary>
    Task NotifyOsdiSubmittedAsync(
        Guid visitId,
        decimal score,
        string severity,
        CancellationToken ct);

    /// <summary>
    /// Notifies the token group that an OSDI questionnaire was submitted.
    /// Used for treatment session tokens where VisitId is null.
    /// </summary>
    Task NotifyTokenSubmittedAsync(
        string token,
        decimal score,
        string severity,
        CancellationToken ct);
}
