namespace Optical.Domain.Enums;

/// <summary>
/// Resolution type for a warranty claim.
/// Note: Replace resolution requires manager approval before processing.
/// </summary>
public enum WarrantyResolution
{
    /// <summary>Replace the defective item with a new one — requires manager approval (Thay thế sản phẩm)</summary>
    Replace = 0,

    /// <summary>Repair the defective item (Sửa chữa)</summary>
    Repair = 1,

    /// <summary>Issue a discount or partial refund to the customer (Chiết khấu)</summary>
    Discount = 2
}
