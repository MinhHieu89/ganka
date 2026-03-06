using Billing.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Billing.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for clinic invoice (Hoa don ban hang).
/// A4 paper size with department-grouped line items and VND formatting.
/// Includes clinic header, patient info, itemized charges, totals, and payment summary.
/// </summary>
public sealed class InvoiceDocument : IDocument
{
    private readonly InvoiceData _data;
    private readonly ClinicHeaderData _header;

    public InvoiceDocument(InvoiceData data, ClinicHeaderData header)
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

            page.Header().Component(new ClinicHeaderComponent(_header));

            page.Content().PaddingTop(8).Column(col =>
            {
                ComposeTitle(col);
                ComposePatientInfo(col);
                ComposeLineItemsTable(col);
                ComposeTotals(col);
                ComposePaymentSummary(col);
            });

            page.Footer().Column(col =>
            {
                ComposeFooter(col);
            });
        });
    }

    private void ComposeTitle(ColumnDescriptor col)
    {
        col.Item().AlignCenter().Text("H\u00d3A \u0110\u01a0N B\u00c1N H\u00c0NG").FontSize(14).Bold();
        col.Item().Height(5);
    }

    private void ComposePatientInfo(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(left =>
            {
                left.Item().Text(text =>
                {
                    text.Span("H\u1ecd t\u00ean: ").Bold();
                    text.Span(_data.PatientName);
                });

                if (!string.IsNullOrWhiteSpace(_data.PatientCode))
                {
                    left.Item().Text(text =>
                    {
                        text.Span("M\u00e3 BN: ").Bold();
                        text.Span(_data.PatientCode);
                    });
                }
            });

            row.RelativeItem().Column(right =>
            {
                right.Item().AlignRight().Text(text =>
                {
                    text.Span("S\u1ed1 HD: ").Bold();
                    text.Span(_data.InvoiceNumber);
                });

                right.Item().AlignRight().Text(text =>
                {
                    text.Span("Ng\u00e0y: ").Bold();
                    text.Span(_data.InvoiceDate.ToString("dd/MM/yyyy"));
                });
            });
        });

        col.Item().Height(8);
    }

    private void ComposeLineItemsTable(ColumnDescriptor col)
    {
        var groupedItems = _data.LineItems
            .GroupBy(i => i.Department)
            .OrderBy(g => (int)g.Key);

        foreach (var group in groupedItems)
        {
            // Department header
            col.Item().Background("#F0F0F0").Padding(4)
                .Text(VndFormatter.GetDepartmentName(group.Key)).FontSize(10).Bold();

            // Items table for this department
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // STT
                    columns.RelativeColumn(4);   // Description
                    columns.ConstantColumn(35);  // Quantity
                    columns.ConstantColumn(80);  // Unit Price
                    columns.ConstantColumn(80);  // Total
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Border(0.5f).Padding(3).Text("STT").FontSize(9).Bold().AlignCenter();
                    header.Cell().Border(0.5f).Padding(3).Text("M\u00f4 t\u1ea3").FontSize(9).Bold();
                    header.Cell().Border(0.5f).Padding(3).Text("SL").FontSize(9).Bold().AlignCenter();
                    header.Cell().Border(0.5f).Padding(3).Text("\u0110\u01a1n gi\u00e1").FontSize(9).Bold().AlignRight();
                    header.Cell().Border(0.5f).Padding(3).Text("Th\u00e0nh ti\u1ec1n").FontSize(9).Bold().AlignRight();
                });

                foreach (var item in group.OrderBy(i => i.SortOrder))
                {
                    table.Cell().Border(0.5f).Padding(3).Text(item.SortOrder.ToString()).FontSize(9).AlignCenter();
                    table.Cell().Border(0.5f).Padding(3).Text(item.Description).FontSize(9);
                    table.Cell().Border(0.5f).Padding(3).Text(item.Quantity.ToString()).FontSize(9).AlignCenter();
                    table.Cell().Border(0.5f).Padding(3).Text(VndFormatter.FormatAmount(item.UnitPrice)).FontSize(9).AlignRight();
                    table.Cell().Border(0.5f).Padding(3).Text(VndFormatter.FormatAmount(item.TotalPrice)).FontSize(9).AlignRight();
                }
            });

            col.Item().Height(4);
        }
    }

    private void ComposeTotals(ColumnDescriptor col)
    {
        col.Item().Height(4);
        col.Item().LineHorizontal(0.5f);
        col.Item().Height(4);

        col.Item().Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(200).Column(totals =>
            {
                totals.Item().Row(r =>
                {
                    r.RelativeItem().Text("T\u1ea1m t\u00ednh:").FontSize(10);
                    r.ConstantItem(100).AlignRight().Text(VndFormatter.FormatAmount(_data.Subtotal)).FontSize(10);
                });

                if (_data.DiscountAmount > 0)
                {
                    totals.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Gi\u1ea3m gi\u00e1:").FontSize(10);
                        r.ConstantItem(100).AlignRight().Text($"-{VndFormatter.FormatAmount(_data.DiscountAmount)}").FontSize(10);
                    });
                }

                totals.Item().Height(3);
                totals.Item().LineHorizontal(1f);
                totals.Item().Height(3);

                totals.Item().Row(r =>
                {
                    r.RelativeItem().Text("T\u1ed4NG C\u1ed8NG:").FontSize(12).Bold();
                    r.ConstantItem(100).AlignRight().Text(VndFormatter.FormatAmountWithCurrency(_data.TotalAmount)).FontSize(12).Bold();
                });
            });
        });
    }

    private void ComposePaymentSummary(ColumnDescriptor col)
    {
        if (_data.Payments.Count == 0) return;

        col.Item().Height(8);
        col.Item().Text("Thanh to\u00e1n:").FontSize(10).Bold();
        col.Item().Height(3);

        foreach (var payment in _data.Payments)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(15);
                row.RelativeItem().Text($"- {VndFormatter.GetPaymentMethodName(payment.Method)}:").FontSize(9);
                row.ConstantItem(100).AlignRight().Text(VndFormatter.FormatAmount(payment.Amount)).FontSize(9);
            });
        }
    }

    private void ComposeFooter(ColumnDescriptor col)
    {
        col.Item().Height(15);
        col.Item().Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(200).Column(right =>
            {
                right.Item().AlignCenter().Text(
                    $"Ng\u00e0y {_data.InvoiceDate:dd} th\u00e1ng {_data.InvoiceDate:MM} n\u0103m {_data.InvoiceDate:yyyy}")
                    .FontSize(9);
                right.Item().AlignCenter().Text("Thu ng\u00e2n").FontSize(10).Bold();
                right.Item().Height(30); // Space for signature
                if (!string.IsNullOrWhiteSpace(_data.CashierName))
                    right.Item().AlignCenter().Text(_data.CashierName).FontSize(10);
            });
        });
    }
}
