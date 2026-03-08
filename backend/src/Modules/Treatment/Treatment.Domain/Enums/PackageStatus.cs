namespace Treatment.Domain.Enums;

/// <summary>
/// Lifecycle status of a treatment package.
/// <para>Valid transitions:</para>
/// <list type="bullet">
///   <item><description>Active -> Paused, PendingCancellation, Switched, Completed (auto when all sessions done)</description></item>
///   <item><description>Paused -> Active (resume)</description></item>
///   <item><description>PendingCancellation -> Cancelled (approved), Active (rejection/reversal)</description></item>
///   <item><description>Cancelled, Switched, Completed -> terminal (no further transitions)</description></item>
/// </list>
/// </summary>
public enum PackageStatus
{
    /// <summary>Active — package is in use; sessions can be scheduled and performed</summary>
    Active = 0,

    /// <summary>Paused — package temporarily suspended; no new sessions until resumed</summary>
    Paused = 1,

    /// <summary>Pending Cancellation — cancellation requested, awaiting manager approval</summary>
    PendingCancellation = 2,

    /// <summary>Cancelled — package cancelled after approval; remaining sessions forfeited or refunded</summary>
    Cancelled = 3,

    /// <summary>Switched — patient switched to a different package; this package is no longer active</summary>
    Switched = 4,

    /// <summary>Completed — all sessions in the package have been performed; terminal status</summary>
    Completed = 5
}
