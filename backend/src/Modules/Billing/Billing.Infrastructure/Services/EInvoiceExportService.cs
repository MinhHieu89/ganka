using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Billing.Infrastructure.Documents.Shared;

namespace Billing.Infrastructure.Services;

/// <summary>
/// Service for exporting e-invoice data in JSON and XML formats
/// suitable for MISA import. Supports both VAT and sales invoices.
/// Uses static methods since the data is already fully assembled in EInvoiceData.
/// </summary>
public static class EInvoiceExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Allows Vietnamese Unicode without escaping
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Exports e-invoice data as JSON string with Vietnamese-friendly options.
    /// Suitable for MISA import and integration with e-invoice providers.
    /// </summary>
    public static string ExportToJson(EInvoiceData data)
    {
        var exportDto = MapToExportDto(data);
        return JsonSerializer.Serialize(exportDto, JsonOptions);
    }

    /// <summary>
    /// Exports e-invoice data as XML string using XDocument/XElement pattern.
    /// Suitable for MISA XML import format.
    /// </summary>
    public static string ExportToXml(EInvoiceData data)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("EInvoice",
                new XElement("InvoiceInfo",
                    new XElement("InvoiceTemplateSymbol", data.InvoiceTemplateSymbol),
                    new XElement("InvoiceSymbol", data.InvoiceSymbol),
                    new XElement("InvoiceNumber", data.InvoiceNumber),
                    new XElement("InvoiceDate", data.InvoiceDate.ToString("yyyy-MM-dd")),
                    new XElement("IsVatInvoice", data.IsVatInvoice)
                ),
                new XElement("Seller",
                    new XElement("Name", data.SellerName),
                    new XElement("TaxCode", data.SellerTaxCode),
                    CreateOptionalElement("Address", data.SellerAddress),
                    CreateOptionalElement("Phone", data.SellerPhone),
                    CreateOptionalElement("BankAccount", data.SellerBankAccount)
                ),
                new XElement("Buyer",
                    new XElement("Name", data.BuyerName),
                    CreateOptionalElement("TaxCode", data.BuyerTaxCode),
                    CreateOptionalElement("Address", data.BuyerAddress)
                ),
                new XElement("LineItems",
                    data.LineItems.OrderBy(i => i.SortOrder).Select(item =>
                        new XElement("Item",
                            new XElement("SortOrder", item.SortOrder),
                            new XElement("Description", item.Description),
                            new XElement("Unit", item.Unit),
                            new XElement("Quantity", item.Quantity),
                            new XElement("UnitPrice", item.UnitPrice),
                            new XElement("Amount", item.Amount)
                        )
                    )
                ),
                new XElement("Totals",
                    new XElement("PreTaxTotal", data.PreTaxTotal),
                    new XElement("TaxRate", data.TaxRate),
                    new XElement("TaxAmount", data.TaxAmount),
                    new XElement("TotalAmount", data.TotalAmount),
                    new XElement("AmountInWords", VndFormatter.AmountToWords(data.TotalAmount))
                ),
                new XElement("Payment",
                    new XElement("Method", data.PaymentMethodText)
                )
            )
        );

        return doc.Declaration + Environment.NewLine + doc.ToString();
    }

    /// <summary>
    /// Maps EInvoiceData to a flat DTO for JSON serialization.
    /// </summary>
    private static EInvoiceExportDto MapToExportDto(EInvoiceData data)
    {
        return new EInvoiceExportDto
        {
            InvoiceTemplateSymbol = data.InvoiceTemplateSymbol,
            InvoiceSymbol = data.InvoiceSymbol,
            InvoiceNumber = data.InvoiceNumber,
            InvoiceDate = data.InvoiceDate.ToString("yyyy-MM-dd"),
            IsVatInvoice = data.IsVatInvoice,
            Seller = new SellerDto
            {
                Name = data.SellerName,
                TaxCode = data.SellerTaxCode,
                Address = data.SellerAddress,
                Phone = data.SellerPhone,
                BankAccount = data.SellerBankAccount
            },
            Buyer = new BuyerDto
            {
                Name = data.BuyerName,
                TaxCode = data.BuyerTaxCode,
                Address = data.BuyerAddress
            },
            LineItems = data.LineItems.OrderBy(i => i.SortOrder).Select(item => new LineItemDto
            {
                SortOrder = item.SortOrder,
                Description = item.Description,
                Unit = item.Unit,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = item.Amount
            }).ToList(),
            PreTaxTotal = data.PreTaxTotal,
            TaxRate = data.TaxRate,
            TaxAmount = data.TaxAmount,
            TotalAmount = data.TotalAmount,
            AmountInWords = VndFormatter.AmountToWords(data.TotalAmount),
            PaymentMethod = data.PaymentMethodText
        };
    }

    /// <summary>
    /// Creates an optional XML element, returning empty content if value is null/empty.
    /// </summary>
    private static object CreateOptionalElement(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new XComment($"{name}: N/A");
        return new XElement(name, value);
    }

    // Internal DTOs for JSON serialization
    private sealed class EInvoiceExportDto
    {
        public string InvoiceTemplateSymbol { get; init; } = default!;
        public string InvoiceSymbol { get; init; } = default!;
        public string InvoiceNumber { get; init; } = default!;
        public string InvoiceDate { get; init; } = default!;
        public bool IsVatInvoice { get; init; }
        public SellerDto Seller { get; init; } = default!;
        public BuyerDto Buyer { get; init; } = default!;
        public List<LineItemDto> LineItems { get; init; } = [];
        public decimal PreTaxTotal { get; init; }
        public decimal TaxRate { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public string AmountInWords { get; init; } = default!;
        public string PaymentMethod { get; init; } = default!;
    }

    private sealed class SellerDto
    {
        public string Name { get; init; } = default!;
        public string TaxCode { get; init; } = default!;
        public string? Address { get; init; }
        public string? Phone { get; init; }
        public string? BankAccount { get; init; }
    }

    private sealed class BuyerDto
    {
        public string Name { get; init; } = default!;
        public string? TaxCode { get; init; }
        public string? Address { get; init; }
    }

    private sealed class LineItemDto
    {
        public int SortOrder { get; init; }
        public string Description { get; init; } = default!;
        public string Unit { get; init; } = default!;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal Amount { get; init; }
    }
}
