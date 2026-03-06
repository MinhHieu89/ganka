namespace Billing.Application.Interfaces;

/// <summary>
/// Service interface for generating billing PDF documents and e-invoice exports.
/// Will be implemented in Plans 15-16 using QuestPDF.
/// </summary>
public interface IBillingDocumentService
{
    /// <summary>
    /// Generates a full invoice PDF for the given invoice.
    /// </summary>
    Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId, CancellationToken ct);

    /// <summary>
    /// Generates a receipt (Phieu thu) PDF for a paid invoice.
    /// </summary>
    Task<byte[]> GenerateReceiptPdfAsync(Guid invoiceId, CancellationToken ct);

    /// <summary>
    /// Generates a Vietnamese e-invoice (hoa don dien tu) PDF per Decree 123/2020.
    /// </summary>
    Task<byte[]> GenerateEInvoicePdfAsync(Guid invoiceId, CancellationToken ct);

    /// <summary>
    /// Generates a shift report PDF for cash reconciliation.
    /// </summary>
    Task<byte[]> GenerateShiftReportPdfAsync(Guid shiftId, CancellationToken ct);

    /// <summary>
    /// Exports e-invoice data as JSON for integration with e-invoice providers.
    /// </summary>
    Task<string> ExportEInvoiceJsonAsync(Guid invoiceId, CancellationToken ct);

    /// <summary>
    /// Exports e-invoice data as XML for integration with e-invoice providers.
    /// </summary>
    Task<string> ExportEInvoiceXmlAsync(Guid invoiceId, CancellationToken ct);
}
