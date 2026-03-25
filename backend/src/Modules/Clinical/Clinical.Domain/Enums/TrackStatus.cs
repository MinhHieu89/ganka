namespace Clinical.Domain.Enums;

/// <summary>
/// Status of a parallel post-payment track (drug or glasses).
/// NotApplicable = not prescribed, Pending = awaiting, InProgress = being processed, Completed = done.
/// </summary>
public enum TrackStatus
{
    NotApplicable = 0,
    Pending = 1,
    InProgress = 2,
    Completed = 3
}
