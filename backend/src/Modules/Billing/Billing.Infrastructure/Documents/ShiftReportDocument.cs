using Billing.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Billing.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for cashier shift report (Bao cao ca lam viec).
/// A4 paper size showing revenue breakdown by payment method and cash reconciliation.
/// Includes shift info, revenue table, cash reconciliation with discrepancy highlighting,
/// and signature lines for cashier and manager.
/// </summary>
public sealed class ShiftReportDocument : IDocument
{
    private readonly ShiftReportData _data;
    private readonly ClinicHeaderData _header;

    public ShiftReportDocument(ShiftReportData data, ClinicHeaderData header)
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
                ComposeShiftInfo(col);
                ComposeRevenueTable(col);
                ComposeRevenueSummary(col);
                ComposeCashReconciliation(col);
            });

            page.Footer().Column(col =>
            {
                ComposeSignatures(col);
            });
        });
    }

    private void ComposeTitle(ColumnDescriptor col)
    {
        col.Item().AlignCenter().Text("B\u00c1O C\u00c1O CA L\u00c0M VI\u1ec6C").FontSize(14).Bold();
        col.Item().Height(6);
    }

    private void ComposeShiftInfo(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(left =>
            {
                left.Item().Text(text =>
                {
                    text.Span("Thu ng\u00e2n: ").Bold();
                    text.Span(_data.CashierName);
                });

                if (!string.IsNullOrWhiteSpace(_data.TemplateName))
                {
                    left.Item().Text(text =>
                    {
                        text.Span("Ca: ").Bold();
                        text.Span(_data.TemplateName);
                    });
                }
            });

            row.RelativeItem().Column(right =>
            {
                right.Item().AlignRight().Text(text =>
                {
                    text.Span("M\u1edf ca: ").Bold();
                    text.Span(_data.OpenedAt.ToString("dd/MM/yyyy HH:mm"));
                });

                if (_data.ClosedAt.HasValue)
                {
                    right.Item().AlignRight().Text(text =>
                    {
                        text.Span("\u0110\u00f3ng ca: ").Bold();
                        text.Span(_data.ClosedAt.Value.ToString("dd/MM/yyyy HH:mm"));
                    });
                }
            });
        });

        col.Item().Height(10);
    }

    private void ComposeRevenueTable(ColumnDescriptor col)
    {
        col.Item().Text("DOANH THU THEO PH\u01af\u01a0NG TH\u1ee8C THANH TO\u00c1N").FontSize(11).Bold();
        col.Item().Height(4);

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Payment method
                columns.ConstantColumn(80);  // Transaction count
                columns.ConstantColumn(120); // Amount
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Border(0.5f).Background("#E8E8E8").Padding(4)
                    .Text("Ph\u01b0\u01a1ng th\u1ee9c").FontSize(10).Bold();
                header.Cell().Border(0.5f).Background("#E8E8E8").Padding(4).AlignCenter()
                    .Text("S\u1ed1 GD").FontSize(10).Bold();
                header.Cell().Border(0.5f).Background("#E8E8E8").Padding(4).AlignRight()
                    .Text("S\u1ed1 ti\u1ec1n").FontSize(10).Bold();
            });

            foreach (var revenue in _data.RevenueByMethod)
            {
                table.Cell().Border(0.5f).Padding(4)
                    .Text(VndFormatter.GetPaymentMethodName(revenue.Method)).FontSize(10);
                table.Cell().Border(0.5f).Padding(4).AlignCenter()
                    .Text(revenue.TransactionCount.ToString()).FontSize(10);
                table.Cell().Border(0.5f).Padding(4).AlignRight()
                    .Text(VndFormatter.FormatAmount(revenue.Amount)).FontSize(10);
            }
        });
    }

    private void ComposeRevenueSummary(ColumnDescriptor col)
    {
        col.Item().Height(6);

        col.Item().Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(280).Column(summary =>
            {
                summary.Item().Row(r =>
                {
                    r.RelativeItem().Text("T\u1ed5ng s\u1ed1 giao d\u1ecbch:").FontSize(10).Bold();
                    r.ConstantItem(100).AlignRight()
                        .Text(_data.TotalTransactions.ToString()).FontSize(10).Bold();
                });

                summary.Item().Row(r =>
                {
                    r.RelativeItem().Text("T\u1ed5ng doanh thu:").FontSize(12).Bold();
                    r.ConstantItem(100).AlignRight()
                        .Text(VndFormatter.FormatAmountWithCurrency(_data.TotalRevenue)).FontSize(12).Bold();
                });
            });
        });

        col.Item().Height(10);
    }

    private void ComposeCashReconciliation(ColumnDescriptor col)
    {
        col.Item().Text("\u0110\u1ed0I CHI\u1ebeU TI\u1ec0N M\u1eb6T").FontSize(11).Bold();
        col.Item().Height(4);

        col.Item().Border(0.5f).Padding(8).Column(reconciliation =>
        {
            ComposeReconciliationRow(reconciliation, "S\u1ed1 d\u01b0 \u0111\u1ea7u ca:", _data.OpeningBalance, false);
            ComposeReconciliationRow(reconciliation, "Ti\u1ec1n m\u1eb7t thu:", _data.CashReceived, false);
            ComposeReconciliationRow(reconciliation, "Ti\u1ec1n m\u1eb7t ho\u00e0n:", _data.CashRefunds, false);

            reconciliation.Item().Height(3);
            reconciliation.Item().LineHorizontal(0.5f);
            reconciliation.Item().Height(3);

            ComposeReconciliationRow(reconciliation, "Ti\u1ec1n m\u1eb7t d\u1ef1 ki\u1ebfn:", _data.ExpectedCash, true);
            ComposeReconciliationRow(reconciliation, "Ti\u1ec1n m\u1eb7t th\u1ef1c t\u1ebf:", _data.ActualCashCount, true);

            reconciliation.Item().Height(3);
            reconciliation.Item().LineHorizontal(1f);
            reconciliation.Item().Height(3);

            // Discrepancy -- highlighted if non-zero
            reconciliation.Item().Row(r =>
            {
                r.RelativeItem().Text("Ch\u00eanh l\u1ec7ch:").FontSize(11).Bold();

                var discrepancyText = r.ConstantItem(120).AlignRight()
                    .Text(VndFormatter.FormatAmountWithCurrency(_data.Discrepancy))
                    .FontSize(11).Bold();

                if (_data.Discrepancy != 0)
                    discrepancyText.FontColor(Colors.Red.Medium);
            });

            // Manager note if discrepancy exists
            if (!string.IsNullOrWhiteSpace(_data.ManagerNote))
            {
                reconciliation.Item().Height(5);
                reconciliation.Item().Text(text =>
                {
                    text.Span("Ghi ch\u00fa qu\u1ea3n l\u00fd: ").FontSize(9).Bold();
                    text.Span(_data.ManagerNote).FontSize(9).Italic();
                });
            }
        });
    }

    private static void ComposeReconciliationRow(ColumnDescriptor col, string label, decimal amount, bool bold)
    {
        col.Item().Row(r =>
        {
            var labelText = r.RelativeItem().Text(label).FontSize(10);
            if (bold) labelText.Bold();

            var amountText = r.ConstantItem(120).AlignRight()
                .Text(VndFormatter.FormatAmount(amount)).FontSize(10);
            if (bold) amountText.Bold();
        });
    }

    private void ComposeSignatures(ColumnDescriptor col)
    {
        col.Item().Height(15);

        var closeDate = _data.ClosedAt ?? DateTime.Now;
        col.Item().AlignCenter().Text(
            $"Ng\u00e0y {closeDate:dd} th\u00e1ng {closeDate:MM} n\u0103m {closeDate:yyyy}")
            .FontSize(9);

        col.Item().Height(5);

        col.Item().Row(row =>
        {
            // Cashier signature
            row.RelativeItem().Column(cashier =>
            {
                cashier.Item().AlignCenter().Text("Thu ng\u00e2n").FontSize(10).Bold();
                cashier.Item().AlignCenter().Text("(K\u00fd, ghi r\u00f5 h\u1ecd t\u00ean)").FontSize(8).Italic();
                cashier.Item().Height(30);
                cashier.Item().AlignCenter().Text(_data.CashierName).FontSize(10);
            });

            // Manager signature
            row.RelativeItem().Column(manager =>
            {
                manager.Item().AlignCenter().Text("Qu\u1ea3n l\u00fd").FontSize(10).Bold();
                manager.Item().AlignCenter().Text("(K\u00fd, ghi r\u00f5 h\u1ecd t\u00ean)").FontSize(8).Italic();
                manager.Item().Height(30);
            });
        });
    }
}
