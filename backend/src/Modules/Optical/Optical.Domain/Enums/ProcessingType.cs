namespace Optical.Domain.Enums;

/// <summary>
/// Indicates where lens processing is performed for a glasses order.
/// Determines estimated delivery time and whether external supplier tracking is required.
/// </summary>
public enum ProcessingType
{
    /// <summary>
    /// In-House (Xử lý tại phòng khám) — simple lenses are cut and fitted at the clinic's optical lab.
    /// Typical turnaround: same day to 1 business day. No supplier coordination needed.
    /// Common for single-vision lenses within standard power ranges.
    /// </summary>
    InHouse = 0,

    /// <summary>
    /// Outsourced (Gửi ra ngoài) — complex lenses (e.g., progressive, high-Rx) are sent to an external
    /// supplier lab (Essilor, Hoya, Viet Phap) for precision grinding and coating.
    /// Typical turnaround: 1–3 business days. Requires supplier order tracking.
    /// </summary>
    Outsourced = 1
}
