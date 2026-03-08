namespace Treatment.Domain.Enums;

/// <summary>
/// Approval status of a cancellation request for a treatment package.
/// Follows the same pattern as Billing.Domain.Enums.RefundStatus.
/// </summary>
public enum CancellationStatus
{
    /// <summary>Cancellation has been requested but not yet reviewed by a manager.</summary>
    Requested = 0,

    /// <summary>Cancellation approved by a manager; package will be cancelled with the specified deduction.</summary>
    Approved = 1,

    /// <summary>Cancellation request rejected by a manager; package remains active.</summary>
    Rejected = 2
}
