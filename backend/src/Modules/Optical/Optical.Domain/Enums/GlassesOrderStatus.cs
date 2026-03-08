namespace Optical.Domain.Enums;

/// <summary>
/// Lifecycle status of a glasses order from creation through delivery.
/// Transitions are linear: Ordered → Processing → Received → Ready → Delivered.
/// Payment must be confirmed before transitioning to Processing (enforced via billing integration).
/// </summary>
public enum GlassesOrderStatus
{
    /// <summary>
    /// Ordered (Đã đặt hàng) — glasses order has been created and linked to the patient's optical prescription.
    /// Awaiting payment confirmation before processing can begin.
    /// </summary>
    Ordered = 0,

    /// <summary>
    /// Processing (Đang xử lý) — payment confirmed; lenses are being prepared (in-house cutting/fitting or
    /// dispatched to supplier lab for outsourced orders). Status transition blocked until full payment received.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Received (Đã nhận hàng) — outsourced lenses have been received back from the supplier lab.
    /// Not applicable for in-house orders (those move directly to Ready).
    /// </summary>
    Received = 2,

    /// <summary>
    /// Ready (Sẵn sàng giao) — glasses are fully assembled and ready for patient pickup.
    /// Staff can contact the patient for collection.
    /// </summary>
    Ready = 3,

    /// <summary>
    /// Delivered (Đã giao) — glasses have been picked up or delivered to the patient.
    /// This is the terminal status; warranty period starts from this date.
    /// </summary>
    Delivered = 4
}
