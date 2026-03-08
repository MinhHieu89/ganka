namespace Treatment.Domain.Enums;

/// <summary>
/// Status of a treatment package cancellation request.
/// Follows the approval workflow: Requested -> Approved/Rejected.
/// </summary>
public enum CancellationRequestStatus
{
    /// <summary>Cancellation has been requested, awaiting manager approval.</summary>
    Requested = 0,

    /// <summary>Manager approved the cancellation.</summary>
    Approved = 1,

    /// <summary>Manager rejected the cancellation request.</summary>
    Rejected = 2
}
