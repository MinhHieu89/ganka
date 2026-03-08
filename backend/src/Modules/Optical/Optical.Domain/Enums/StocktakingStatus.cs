namespace Optical.Domain.Enums;

/// <summary>
/// Lifecycle status of a stocktaking (inventory count) session in the optical center.
/// </summary>
public enum StocktakingStatus
{
    /// <summary>Stocktaking session is currently in progress (Đang kiểm kho)</summary>
    InProgress = 0,

    /// <summary>Stocktaking session has been completed and adjustments applied (Hoàn thành)</summary>
    Completed = 1,

    /// <summary>Stocktaking session was cancelled before completion (Đã hủy)</summary>
    Cancelled = 2
}
