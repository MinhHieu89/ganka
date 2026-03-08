namespace Optical.Domain.Enums;

/// <summary>
/// Approval status for a warranty claim that requires manager authorization.
/// Used when the warranty resolution type is Replace.
/// </summary>
public enum WarrantyApprovalStatus
{
    /// <summary>Warranty claim is awaiting manager review (Đang chờ duyệt)</summary>
    Pending = 0,

    /// <summary>Warranty claim has been approved by manager (Đã duyệt)</summary>
    Approved = 1,

    /// <summary>Warranty claim has been rejected by manager (Từ chối)</summary>
    Rejected = 2
}
