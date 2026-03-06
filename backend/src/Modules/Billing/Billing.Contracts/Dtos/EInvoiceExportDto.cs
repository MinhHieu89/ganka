namespace Billing.Contracts.Dtos;

/// <summary>
/// Export DTO containing all fields required by Vietnamese e-invoice regulations
/// per Decree 123/2020/ND-CP and Circular 32/2025/TT-BTC.
/// Used to generate compliant electronic invoices for tax authority submission.
/// </summary>
public sealed record EInvoiceExportDto(
    /// <summary>Ky hieu mau so hoa don (Invoice template name, e.g., "HOA DON GIA TRI GIA TANG").</summary>
    string InvoiceTemplateName,
    /// <summary>Ky hieu mau so hoa don (Invoice template symbol, e.g., "1C26TBB").</summary>
    string InvoiceTemplateSymbol,
    /// <summary>Ky hieu hoa don (Invoice symbol, e.g., "AA/26E").</summary>
    string InvoiceSymbol,
    /// <summary>So hoa don (Invoice number).</summary>
    string InvoiceNumber,
    /// <summary>Ngay, thang, nam (Date of issue).</summary>
    DateTime DateOfIssue,
    /// <summary>Ten nguoi ban (Seller name).</summary>
    string SellerName,
    /// <summary>Ma so thue nguoi ban (Seller tax code).</summary>
    string SellerTaxCode,
    /// <summary>Dia chi nguoi ban (Seller address).</summary>
    string SellerAddress,
    /// <summary>Ten nguoi mua (Buyer name).</summary>
    string BuyerName,
    /// <summary>Ma so thue nguoi mua (Buyer tax code -- required if buyer is a business entity).</summary>
    string? BuyerTaxCode,
    /// <summary>Dia chi nguoi mua (Buyer address).</summary>
    string? BuyerAddress,
    /// <summary>Line items on the e-invoice.</summary>
    List<EInvoiceLineItemDto> Items,
    /// <summary>Tong tien chua thue (Pre-tax total).</summary>
    decimal PreTaxTotal,
    /// <summary>Thue suat (Tax rate -- 8% default for healthcare services, configurable).</summary>
    decimal TaxRate,
    /// <summary>Tien thue (Tax amount).</summary>
    decimal TaxAmount,
    /// <summary>Tong tien thanh toan (Total amount including tax).</summary>
    decimal TotalAmount,
    /// <summary>Currency code (always "VND" for Vietnamese invoices).</summary>
    string Currency,
    /// <summary>Hinh thuc thanh toan (Payment method description).</summary>
    string PaymentMethod);

/// <summary>
/// Individual line item on a Vietnamese e-invoice.
/// </summary>
public sealed record EInvoiceLineItemDto(
    /// <summary>Ten hang hoa, dich vu (Item description).</summary>
    string Description,
    /// <summary>Don vi tinh (Unit of measurement).</summary>
    string? Unit,
    /// <summary>So luong (Quantity).</summary>
    int Quantity,
    /// <summary>Don gia (Unit price).</summary>
    decimal UnitPrice,
    /// <summary>Thanh tien (Line amount = Quantity x UnitPrice).</summary>
    decimal Amount);
