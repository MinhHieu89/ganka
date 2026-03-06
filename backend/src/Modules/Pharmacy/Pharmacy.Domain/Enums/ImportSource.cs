namespace Pharmacy.Domain.Enums;

/// <summary>
/// Indicates the source of a stock import transaction.
/// Used for audit trails and UI filtering of import history.
/// </summary>
public enum ImportSource
{
    /// <summary>
    /// Stock imported by entering a physical supplier invoice through the stock import form.
    /// Day-to-day operation for routine drug restocking.
    /// </summary>
    SupplierInvoice = 0,

    /// <summary>
    /// Stock imported in bulk via an Excel spreadsheet upload.
    /// Used for large initial stock loads or periodic bulk restocking.
    /// </summary>
    ExcelBulk = 1
}
