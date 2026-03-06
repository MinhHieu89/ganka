using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Billing.Infrastructure.Documents;
using Billing.Infrastructure.Documents.Shared;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Shared.Application.Interfaces;

namespace Billing.Infrastructure.Services;

/// <summary>
/// Service for generating billing PDF documents (invoices, receipts, e-invoices, shift reports)
/// and exporting e-invoice data in JSON/XML formats for MISA integration.
/// Follows the same pattern as Clinical.Infrastructure DocumentService.
/// </summary>
public sealed class BillingDocumentService : IBillingDocumentService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICashierShiftRepository _shiftRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IClinicSettingsService _clinicSettingsService;
    private readonly BillingDbContext _billingDb;

    /// <summary>Default VAT tax rate for Vietnamese clinic services (8%).</summary>
    private const decimal DefaultTaxRate = 8m;

    public BillingDocumentService(
        IInvoiceRepository invoiceRepository,
        ICashierShiftRepository shiftRepository,
        IPaymentRepository paymentRepository,
        IClinicSettingsService clinicSettingsService,
        BillingDbContext billingDb)
    {
        _invoiceRepository = invoiceRepository;
        _shiftRepository = shiftRepository;
        _paymentRepository = paymentRepository;
        _clinicSettingsService = clinicSettingsService;
        _billingDb = billingDb;

        // Ensure fonts are registered (idempotent, thread-safe)
        DocumentFontManager.RegisterFonts();
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, ct)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");

        var header = await GetClinicHeaderDataAsync(ct);
        var patientCode = await GetPatientCodeAsync(invoice.PatientId, ct);

        var lineItems = invoice.LineItems
            .OrderBy(li => li.Department).ThenBy(li => li.CreatedAt)
            .Select((li, index) => new InvoiceLineItemData(
                SortOrder: index + 1,
                Department: li.Department,
                Description: li.DescriptionVi ?? li.Description,
                DescriptionEn: li.Description,
                Quantity: li.Quantity,
                Unit: GetDefaultUnit(li.Department),
                UnitPrice: li.UnitPrice,
                TotalPrice: li.LineTotal))
            .ToList();

        var payments = invoice.Payments
            .Where(p => p.Status == PaymentStatus.Confirmed)
            .Select(p => new InvoicePaymentData(
                Method: p.Method,
                Amount: p.Amount,
                ConfirmedAt: p.UpdatedAt))
            .ToList();

        var data = new InvoiceData(
            InvoiceNumber: invoice.InvoiceNumber,
            InvoiceDate: invoice.FinalizedAt ?? invoice.CreatedAt,
            PatientName: invoice.PatientName,
            PatientCode: patientCode,
            LineItems: lineItems,
            Payments: payments,
            Subtotal: invoice.SubTotal,
            DiscountAmount: invoice.DiscountTotal,
            TotalAmount: invoice.TotalAmount,
            CashierName: null);

        var document = new InvoiceDocument(data, header);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateReceiptPdfAsync(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, ct)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");

        var header = await GetClinicHeaderDataAsync(ct);
        var patientCode = await GetPatientCodeAsync(invoice.PatientId, ct);

        var payments = invoice.Payments
            .Where(p => p.Status == PaymentStatus.Confirmed)
            .Select(p => new InvoicePaymentData(
                Method: p.Method,
                Amount: p.Amount,
                ConfirmedAt: p.UpdatedAt))
            .ToList();

        var data = new ReceiptData(
            InvoiceNumber: invoice.InvoiceNumber,
            ReceiptDate: invoice.FinalizedAt ?? DateTime.UtcNow,
            PatientName: invoice.PatientName,
            PatientCode: patientCode,
            TotalAmount: invoice.TotalAmount,
            Payments: payments,
            CashierName: null);

        var document = new ReceiptDocument(data, header);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateEInvoicePdfAsync(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, ct)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");

        var header = await GetClinicHeaderDataAsync(ct);
        var eInvoiceData = BuildEInvoiceData(invoice, header);

        var document = new EInvoiceDocument(eInvoiceData, header);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateShiftReportPdfAsync(Guid shiftId, CancellationToken ct)
    {
        var shift = await _shiftRepository.GetByIdAsync(shiftId, ct)
            ?? throw new InvalidOperationException($"Cashier shift {shiftId} not found.");

        var header = await GetClinicHeaderDataAsync(ct);
        var payments = await _paymentRepository.GetByShiftIdAsync(shiftId, ct);

        // Get template name if linked
        string? templateName = null;
        if (shift.ShiftTemplateId.HasValue)
        {
            templateName = await _billingDb.ShiftTemplates
                .AsNoTracking()
                .Where(t => t.Id == shift.ShiftTemplateId.Value)
                .Select(t => t.NameVi ?? t.Name)
                .FirstOrDefaultAsync(ct);
        }

        var confirmedPayments = payments
            .Where(p => p.Status == PaymentStatus.Confirmed)
            .ToList();

        var revenueByMethod = confirmedPayments
            .GroupBy(p => p.Method)
            .Select(g => new ShiftRevenueByMethodData(
                Method: g.Key,
                TransactionCount: g.Count(),
                Amount: g.Sum(p => p.Amount)))
            .OrderBy(r => (int)r.Method)
            .ToList();

        var data = new ShiftReportData(
            CashierName: shift.CashierName,
            TemplateName: templateName,
            OpenedAt: shift.OpenedAt,
            ClosedAt: shift.ClosedAt,
            RevenueByMethod: revenueByMethod,
            TotalTransactions: shift.TransactionCount,
            TotalRevenue: shift.TotalRevenue,
            OpeningBalance: shift.OpeningBalance,
            CashReceived: shift.CashReceived,
            CashRefunds: shift.CashRefunds,
            ExpectedCash: shift.ExpectedCashAmount,
            ActualCashCount: shift.ActualCashCount ?? 0m,
            Discrepancy: shift.Discrepancy ?? 0m,
            ManagerNote: shift.ManagerNote);

        var document = new ShiftReportDocument(data, header);
        return document.GeneratePdf();
    }

    public async Task<string> ExportEInvoiceJsonAsync(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, ct)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");

        var header = await GetClinicHeaderDataAsync(ct);
        var eInvoiceData = BuildEInvoiceData(invoice, header);

        return EInvoiceExportService.ExportToJson(eInvoiceData);
    }

    public async Task<string> ExportEInvoiceXmlAsync(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, ct)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");

        var header = await GetClinicHeaderDataAsync(ct);
        var eInvoiceData = BuildEInvoiceData(invoice, header);

        return EInvoiceExportService.ExportToXml(eInvoiceData);
    }

    /// <summary>
    /// Builds the e-invoice data record from an invoice and clinic header.
    /// Includes tax calculation and all Decree 123/2020 mandatory fields.
    /// </summary>
    private static EInvoiceData BuildEInvoiceData(Invoice invoice, ClinicHeaderData header)
    {
        var lineItems = invoice.LineItems
            .OrderBy(li => li.Department).ThenBy(li => li.CreatedAt)
            .Select((li, index) => new EInvoiceLineItemData(
                SortOrder: index + 1,
                Description: li.DescriptionVi ?? li.Description,
                Unit: GetDefaultUnit(li.Department),
                Quantity: li.Quantity,
                UnitPrice: li.UnitPrice,
                Amount: li.LineTotal))
            .ToList();

        var preTaxTotal = invoice.SubTotal - invoice.DiscountTotal;
        var taxRate = DefaultTaxRate;
        var taxAmount = Math.Round(preTaxTotal * taxRate / 100m, 0);
        var totalWithTax = preTaxTotal + taxAmount;

        // Determine payment method text from invoice payments
        var confirmedPayments = invoice.Payments
            .Where(p => p.Status == PaymentStatus.Confirmed)
            .ToList();

        var paymentMethodText = confirmedPayments.Count switch
        {
            0 => "Chua thanh toan",
            1 => VndFormatter.GetPaymentMethodName(confirmedPayments[0].Method),
            _ => string.Join(", ", confirmedPayments.Select(p => VndFormatter.GetPaymentMethodName(p.Method)).Distinct())
        };

        return new EInvoiceData(
            InvoiceTemplateSymbol: "1/001",
            InvoiceSymbol: "C26T",
            InvoiceNumber: invoice.InvoiceNumber,
            InvoiceDate: invoice.FinalizedAt ?? invoice.CreatedAt,
            SellerName: header.ClinicNameVi ?? header.ClinicName,
            SellerTaxCode: header.TaxCode ?? "N/A",
            SellerAddress: header.Address,
            SellerPhone: header.Phone,
            SellerBankAccount: null,
            BuyerName: invoice.PatientName,
            BuyerTaxCode: null,
            BuyerAddress: null,
            LineItems: lineItems,
            PreTaxTotal: preTaxTotal,
            TaxRate: taxRate,
            TaxAmount: taxAmount,
            TotalAmount: totalWithTax,
            PaymentMethodText: paymentMethodText,
            IsVatInvoice: true);
    }

    /// <summary>
    /// Gets the clinic header data from IClinicSettingsService with sensible defaults.
    /// </summary>
    private async Task<ClinicHeaderData> GetClinicHeaderDataAsync(CancellationToken ct)
    {
        var settings = await _clinicSettingsService.GetCurrentAsync(ct);

        if (settings is null)
        {
            return new ClinicHeaderData(
                ClinicName: "GANKA Eye Clinic",
                ClinicNameVi: "PHONG KHAM MAT GANKA",
                Address: null,
                Phone: null,
                Fax: null,
                TaxCode: null,
                LicenseNumber: null,
                Tagline: null,
                LogoBytes: null);
        }

        return new ClinicHeaderData(
            ClinicName: settings.ClinicName,
            ClinicNameVi: settings.ClinicNameVi,
            Address: settings.Address,
            Phone: settings.Phone,
            Fax: settings.Fax,
            TaxCode: null, // TaxCode not yet in ClinicSettingsDto; will be added when settings are extended
            LicenseNumber: settings.LicenseNumber,
            Tagline: settings.Tagline,
            LogoBytes: null);
    }

    /// <summary>
    /// Queries patient code from patient schema via raw SQL (cross-module boundary).
    /// </summary>
    private async Task<string?> GetPatientCodeAsync(Guid patientId, CancellationToken ct)
    {
        var results = await _billingDb.Database
            .SqlQuery<PatientCodeResult>(
                $"SELECT [PatientCode] FROM [patient].[Patients] WHERE [Id] = {patientId}")
            .ToListAsync(ct);

        return results.FirstOrDefault()?.PatientCode;
    }

    /// <summary>
    /// Returns a default unit of measure based on department.
    /// Since InvoiceLineItem doesn't track unit, we infer from department.
    /// </summary>
    private static string GetDefaultUnit(Department department) => department switch
    {
        Department.Medical => "Lan",
        Department.Pharmacy => "Hop",
        Department.Optical => "Cai",
        Department.Treatment => "Lan",
        _ => "Lan"
    };

    /// <summary>Minimal record for patient code cross-schema query.</summary>
    private sealed record PatientCodeResult(string? PatientCode);
}
