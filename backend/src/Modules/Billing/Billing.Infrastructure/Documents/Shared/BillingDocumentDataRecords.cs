using Billing.Domain.Enums;

namespace Billing.Infrastructure.Documents.Shared;

/// <summary>
/// Clinic header data for all billing document types.
/// Populated from ClinicSettings or defaults if no settings exist.
/// Extended with TaxCode for e-invoice compliance.
/// </summary>
public sealed record ClinicHeaderData(
    string ClinicName,
    string? ClinicNameVi,
    string? Address,
    string? Phone,
    string? Fax,
    string? TaxCode,
    string? LicenseNumber,
    string? Tagline,
    byte[]? LogoBytes);

/// <summary>
/// Data record for invoice PDF document generation.
/// Groups line items by department for Vietnamese billing format.
/// </summary>
public sealed record InvoiceData(
    string InvoiceNumber,
    DateTime InvoiceDate,
    string PatientName,
    string? PatientCode,
    List<InvoiceLineItemData> LineItems,
    List<InvoicePaymentData> Payments,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? CashierName);

/// <summary>
/// Individual line item in an invoice, tagged with department for grouping.
/// </summary>
public sealed record InvoiceLineItemData(
    int SortOrder,
    Department Department,
    string Description,
    string? DescriptionEn,
    int Quantity,
    string Unit,
    decimal UnitPrice,
    decimal TotalPrice);

/// <summary>
/// Payment record associated with an invoice.
/// </summary>
public sealed record InvoicePaymentData(
    PaymentMethod Method,
    decimal Amount,
    DateTime? ConfirmedAt);

/// <summary>
/// Data record for receipt (Phieu thu) document generation.
/// Simplified view of an invoice payment confirmation.
/// </summary>
public sealed record ReceiptData(
    string InvoiceNumber,
    DateTime ReceiptDate,
    string PatientName,
    string? PatientCode,
    decimal TotalAmount,
    List<InvoicePaymentData> Payments,
    string? CashierName);

/// <summary>
/// Data record for Vietnamese e-invoice (hoa don dien tu) per Decree 123/2020.
/// Includes all mandatory fields required by Vietnamese tax law.
/// </summary>
public sealed record EInvoiceData(
    string InvoiceTemplateSymbol,
    string InvoiceSymbol,
    string InvoiceNumber,
    DateTime InvoiceDate,
    // Seller info
    string SellerName,
    string SellerTaxCode,
    string? SellerAddress,
    string? SellerPhone,
    string? SellerBankAccount,
    // Buyer info
    string BuyerName,
    string? BuyerTaxCode,
    string? BuyerAddress,
    // Line items
    List<EInvoiceLineItemData> LineItems,
    // Totals
    decimal PreTaxTotal,
    decimal TaxRate,
    decimal TaxAmount,
    decimal TotalAmount,
    // Payment
    string PaymentMethodText,
    // E-invoice type: true = VAT invoice, false = sales invoice
    bool IsVatInvoice);

/// <summary>
/// Individual line item in an e-invoice.
/// </summary>
public sealed record EInvoiceLineItemData(
    int SortOrder,
    string Description,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal Amount);

/// <summary>
/// Data record for shift report (Bao cao ca lam viec) document generation.
/// Shows revenue by payment method and cash reconciliation.
/// </summary>
public sealed record ShiftReportData(
    string CashierName,
    string? TemplateName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    List<ShiftRevenueByMethodData> RevenueByMethod,
    int TotalTransactions,
    decimal TotalRevenue,
    // Cash reconciliation
    decimal OpeningBalance,
    decimal CashReceived,
    decimal CashRefunds,
    decimal ExpectedCash,
    decimal ActualCashCount,
    decimal Discrepancy,
    string? ManagerNote);

/// <summary>
/// Revenue breakdown per payment method in a shift report.
/// </summary>
public sealed record ShiftRevenueByMethodData(
    PaymentMethod Method,
    int TransactionCount,
    decimal Amount);
