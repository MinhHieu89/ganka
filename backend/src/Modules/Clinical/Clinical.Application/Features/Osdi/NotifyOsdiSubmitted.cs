using Clinical.Application.Interfaces;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine cascading handler that notifies the visit group via SignalR
/// after an OSDI questionnaire is successfully submitted.
/// Fire-and-forget: delegates to IOsdiNotificationService which swallows errors.
/// </summary>
public sealed record OsdiSubmittedEvent(Guid VisitId, decimal Score, string Severity);

public static class NotifyOsdiSubmittedHandler
{
    public static async Task Handle(
        OsdiSubmittedEvent @event,
        IOsdiNotificationService notificationService,
        CancellationToken ct)
    {
        await notificationService.NotifyOsdiSubmittedAsync(
            @event.VisitId,
            @event.Score,
            @event.Severity,
            ct);
    }
}
