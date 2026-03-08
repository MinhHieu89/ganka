namespace Treatment.Domain.Enums;

/// <summary>
/// Status of an individual treatment session within a package or standalone booking.
/// Transitions: Scheduled -> InProgress -> Completed, or Scheduled -> Cancelled.
/// </summary>
public enum SessionStatus
{
    /// <summary>Scheduled — session is booked but has not started</summary>
    Scheduled = 0,

    /// <summary>In Progress — session is currently being performed</summary>
    InProgress = 1,

    /// <summary>Completed — session finished successfully; clinical notes recorded</summary>
    Completed = 2,

    /// <summary>Cancelled — session was cancelled before or during execution</summary>
    Cancelled = 3
}
