using Billing.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Billing.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for Vietnamese e-invoice (hoa don dien tu) per Decree 123/2020.
/// A4 paper size with all mandatory fields required by Vietnamese tax law:
/// - Invoice template/symbol identifiers
/// - Seller and buyer tax information
/// - Itemized line items with unit, quantity, price, amount
/// - Tax breakdown (pre-tax, rate, tax amount, total)
/// - Amount in Vietnamese words
/// - Buyer/seller signature areas
/// </summary>
public sealed class EInvoiceDocument : IDocument
{
    private readonly EInvoiceData _data;
    private readonly ClinicHeaderData _header;

    public EInvoiceDocument(EInvoiceData data, ClinicHeaderData header)
    {
        _data = data;
        _header = header;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(15, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Noto Sans"));

            page.Header().Column(col =>
            {
                ComposeEInvoiceHeader(col);
            });

            page.Content().PaddingTop(8).Column(col =>
            {
                ComposeSellerBuyerInfo(col);
                ComposeLineItemsTable(col);
                ComposeTaxSection(col);
                ComposeAmountInWords(col);
                ComposePaymentMethod(col);
            });

            page.Footer().Column(col =>
            {
                ComposeSignatures(col);
            });
        });
    }

    private void ComposeEInvoiceHeader(ColumnDescriptor col)
    {
        // E-invoice title
        var title = _data.IsVatInvoice
            ? "H\u00d3A \u0110\u01a0N GI\u00c1 TR\u1eca GIA T\u0102NG"
            : "H\u00d3A \u0110\u01a0N B\u00c1N H\u00c0NG";

        col.Item().AlignCenter().Text(title).FontSize(15).Bold();
        col.Item().AlignCenter().Text("(E-Invoice / H\u00f3a \u0111\u01a1n \u0111i\u1ec7n t\u1eed)").FontSize(9).Italic();
        col.Item().Height(3);

        // Invoice identifiers
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(left =>
            {
                left.Item().Text(text =>
                {
                    text.Span("K\u00fd hi\u1ec7u m\u1eabu s\u1ed1: ").FontSize(9);
                    text.Span(_data.InvoiceTemplateSymbol).FontSize(9).Bold();
                });
                left.Item().Text(text =>
                {
                    text.Span("K\u00fd hi\u1ec7u: ").FontSize(9);
                    text.Span(_data.InvoiceSymbol).FontSize(9).Bold();
                });
            });

            row.RelativeItem().Column(right =>
            {
                right.Item().AlignRight().Text(text =>
                {
                    text.Span("S\u1ed1: ").FontSize(9);
                    text.Span(_data.InvoiceNumber).FontSize(9).Bold();
                });
                right.Item().AlignRight().Text(text =>
                {
                    text.Span("Ng\u00e0y: ").FontSize(9);
                    text.Span(_data.InvoiceDate.ToString("dd/MM/yyyy")).FontSize(9);
                });
            });
        });
    }

    private void ComposeSellerBuyerInfo(ColumnDescriptor col)
    {
        // Seller section
        col.Item().Text("\u0110\u01a1n v\u1ecb b\u00e1n h\u00e0ng (Seller):").FontSize(10).Bold();

        col.Item().Text(text =>
        {
            text.Span("T\u00ean: ").FontSize(9);
            text.Span(_data.SellerName).FontSize(9);
        });

        col.Item().Text(text =>
        {
            text.Span("M\u00e3 s\u1ed1 thu\u1ebf (MST): ").FontSize(9);
            text.Span(_data.SellerTaxCode).FontSize(9).Bold();
        });

        if (!string.IsNullOrWhiteSpace(_data.SellerAddress))
        {
            col.Item().Text(text =>
            {
                text.Span("\u0110\u1ecba ch\u1ec9: ").FontSize(9);
                text.Span(_data.SellerAddress).FontSize(9);
            });
        }

        if (!string.IsNullOrWhiteSpace(_data.SellerPhone))
        {
            col.Item().Text(text =>
            {
                text.Span("\u0110i\u1ec7n tho\u1ea1i: ").FontSize(9);
                text.Span(_data.SellerPhone).FontSize(9);
            });
        }

        if (!string.IsNullOrWhiteSpace(_data.SellerBankAccount))
        {
            col.Item().Text(text =>
            {
                text.Span("S\u1ed1 TK ng\u00e2n h\u00e0ng: ").FontSize(9);
                text.Span(_data.SellerBankAccount).FontSize(9);
            });
        }

        col.Item().Height(5);

        // Buyer section
        col.Item().Text("Ng\u01b0\u1eddi mua h\u00e0ng (Buyer):").FontSize(10).Bold();

        col.Item().Text(text =>
        {
            text.Span("T\u00ean: ").FontSize(9);
            text.Span(_data.BuyerName).FontSize(9);
        });

        if (!string.IsNullOrWhiteSpace(_data.BuyerTaxCode))
        {
            col.Item().Text(text =>
            {
                text.Span("M\u00e3 s\u1ed1 thu\u1ebf (MST): ").FontSize(9);
                text.Span(_data.BuyerTaxCode).FontSize(9).Bold();
            });
        }

        if (!string.IsNullOrWhiteSpace(_data.BuyerAddress))
        {
            col.Item().Text(text =>
            {
                text.Span("\u0110\u1ecba ch\u1ec9: ").FontSize(9);
                text.Span(_data.BuyerAddress).FontSize(9);
            });
        }

        col.Item().Height(8);
    }

    private void ComposeLineItemsTable(ColumnDescriptor col)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);  // STT
                columns.RelativeColumn(4);   // Description
                columns.ConstantColumn(45);  // Unit (Don vi tinh)
                columns.ConstantColumn(35);  // Quantity (So luong)
                columns.ConstantColumn(75);  // Unit Price (Don gia)
                columns.ConstantColumn(80);  // Amount (Thanh tien)
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Border(0.5f).Padding(3).AlignCenter()
                    .Text("STT").FontSize(9).Bold();
                header.Cell().Border(0.5f).Padding(3)
                    .Text("T\u00ean h\u00e0ng h\u00f3a, d\u1ecbch v\u1ee5").FontSize(9).Bold();
                header.Cell().Border(0.5f).Padding(3).AlignCenter()
                    .Text("\u0110VT").FontSize(9).Bold();
                header.Cell().Border(0.5f).Padding(3).AlignCenter()
                    .Text("S\u1ed1 l\u01b0\u1ee3ng").FontSize(9).Bold();
                header.Cell().Border(0.5f).Padding(3).AlignRight()
                    .Text("\u0110\u01a1n gi\u00e1").FontSize(9).Bold();
                header.Cell().Border(0.5f).Padding(3).AlignRight()
                    .Text("Th\u00e0nh ti\u1ec1n").FontSize(9).Bold();
            });

            foreach (var item in _data.LineItems.OrderBy(i => i.SortOrder))
            {
                table.Cell().Border(0.5f).Padding(3).AlignCenter()
                    .Text(item.SortOrder.ToString()).FontSize(9);
                table.Cell().Border(0.5f).Padding(3)
                    .Text(item.Description).FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignCenter()
                    .Text(item.Unit).FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignCenter()
                    .Text(item.Quantity.ToString()).FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignRight()
                    .Text(VndFormatter.FormatAmount(item.UnitPrice)).FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignRight()
                    .Text(VndFormatter.FormatAmount(item.Amount)).FontSize(9);
            }
        });
    }

    private void ComposeTaxSection(ColumnDescriptor col)
    {
        col.Item().Height(6);

        col.Item().Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(280).Column(totals =>
            {
                // Pre-tax total
                totals.Item().Row(r =>
                {
                    r.RelativeItem().Text("C\u1ed9ng ti\u1ec1n h\u00e0ng:").FontSize(10);
                    r.ConstantItem(100).AlignRight()
                        .Text(VndFormatter.FormatAmount(_data.PreTaxTotal)).FontSize(10);
                });

                if (_data.IsVatInvoice)
                {
                    // Tax rate
                    totals.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Thu\u1ebf su\u1ea5t GTGT: {_data.TaxRate:0}%").FontSize(10);
                        r.ConstantItem(100).AlignRight()
                            .Text(VndFormatter.FormatAmount(_data.TaxAmount)).FontSize(10);
                    });
                }

                totals.Item().Height(3);
                totals.Item().LineHorizontal(1f);
                totals.Item().Height(3);

                // Total
                totals.Item().Row(r =>
                {
                    r.RelativeItem().Text("T\u1ed5ng ti\u1ec1n thanh to\u00e1n:").FontSize(12).Bold();
                    r.ConstantItem(100).AlignRight()
                        .Text(VndFormatter.FormatAmountWithCurrency(_data.TotalAmount)).FontSize(12).Bold();
                });
            });
        });
    }

    private void ComposeAmountInWords(ColumnDescriptor col)
    {
        col.Item().Height(6);
        col.Item().Text(text =>
        {
            text.Span("S\u1ed1 ti\u1ec1n vi\u1ebft b\u1eb1ng ch\u1eef: ").FontSize(10).Bold();
            text.Span(VndFormatter.AmountToWords(_data.TotalAmount)).FontSize(10).Italic();
        });
    }

    private void ComposePaymentMethod(ColumnDescriptor col)
    {
        col.Item().Height(3);
        col.Item().Text(text =>
        {
            text.Span("H\u00ecnh th\u1ee9c thanh to\u00e1n: ").FontSize(9);
            text.Span(_data.PaymentMethodText).FontSize(9);
        });
    }

    private void ComposeSignatures(ColumnDescriptor col)
    {
        col.Item().Height(10);
        col.Item().Row(row =>
        {
            // Buyer signature
            row.RelativeItem().Column(buyer =>
            {
                buyer.Item().AlignCenter().Text("Ng\u01b0\u1eddi mua h\u00e0ng").FontSize(10).Bold();
                buyer.Item().AlignCenter().Text("(K\u00fd, ghi r\u00f5 h\u1ecd t\u00ean)").FontSize(8).Italic();
                buyer.Item().Height(35);
            });

            // Seller signature
            row.RelativeItem().Column(seller =>
            {
                seller.Item().AlignCenter().Text("Ng\u01b0\u1eddi b\u00e1n h\u00e0ng").FontSize(10).Bold();
                seller.Item().AlignCenter().Text("(K\u00fd, \u0111\u00f3ng d\u1ea5u, ghi r\u00f5 h\u1ecd t\u00ean)").FontSize(8).Italic();
                seller.Item().Height(35);
            });
        });
    }
}
