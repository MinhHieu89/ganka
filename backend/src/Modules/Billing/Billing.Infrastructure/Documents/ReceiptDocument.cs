using Billing.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Billing.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for payment receipt (Phieu thu).
/// A5 paper size -- compact confirmation of payment received.
/// Includes clinic header, patient info, total, payment methods, and cashier signature.
/// </summary>
public sealed class ReceiptDocument : IDocument
{
    private readonly ReceiptData _data;
    private readonly ClinicHeaderData _header;

    public ReceiptDocument(ReceiptData data, ClinicHeaderData header)
    {
        _data = data;
        _header = header;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.Margin(10, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Noto Sans"));

            page.Header().Component(new ClinicHeaderComponent(_header));

            page.Content().PaddingTop(6).Column(col =>
            {
                ComposeTitle(col);
                ComposeDetails(col);
                ComposePayments(col);
            });

            page.Footer().Column(col =>
            {
                ComposeFooter(col);
            });
        });
    }

    private void ComposeTitle(ColumnDescriptor col)
    {
        col.Item().AlignCenter().Text("PHI\u1ebeU THU").FontSize(13).Bold();
        col.Item().Height(4);
    }

    private void ComposeDetails(ColumnDescriptor col)
    {
        col.Item().Text(text =>
        {
            text.Span("H\u1ecd t\u00ean: ").Bold();
            text.Span(_data.PatientName);
        });

        if (!string.IsNullOrWhiteSpace(_data.PatientCode))
        {
            col.Item().Text(text =>
            {
                text.Span("M\u00e3 BN: ").Bold();
                text.Span(_data.PatientCode);
            });
        }

        col.Item().Text(text =>
        {
            text.Span("S\u1ed1 HD: ").Bold();
            text.Span(_data.InvoiceNumber);
        });

        col.Item().Text(text =>
        {
            text.Span("Ng\u00e0y: ").Bold();
            text.Span(_data.ReceiptDate.ToString("dd/MM/yyyy HH:mm"));
        });

        col.Item().Height(6);

        // Total amount
        col.Item().Background("#F5F5F5").Padding(6).Row(row =>
        {
            row.RelativeItem().Text("T\u1ed5ng ti\u1ec1n:").FontSize(12).Bold();
            row.ConstantItem(120).AlignRight()
                .Text(VndFormatter.FormatAmountWithCurrency(_data.TotalAmount)).FontSize(12).Bold();
        });

        col.Item().Height(6);
    }

    private void ComposePayments(ColumnDescriptor col)
    {
        if (_data.Payments.Count == 0) return;

        col.Item().Text("Ph\u01b0\u01a1ng th\u1ee9c thanh to\u00e1n:").FontSize(9).Bold();
        col.Item().Height(3);

        foreach (var payment in _data.Payments)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(10);
                row.RelativeItem().Text($"- {VndFormatter.GetPaymentMethodName(payment.Method)}:").FontSize(9);
                row.ConstantItem(80).AlignRight().Text(VndFormatter.FormatAmount(payment.Amount)).FontSize(9);
            });
        }
    }

    private void ComposeFooter(ColumnDescriptor col)
    {
        col.Item().Height(10);
        col.Item().Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(160).Column(right =>
            {
                right.Item().AlignCenter().Text(
                    $"Ng\u00e0y {_data.ReceiptDate:dd} th\u00e1ng {_data.ReceiptDate:MM} n\u0103m {_data.ReceiptDate:yyyy}")
                    .FontSize(8);
                right.Item().AlignCenter().Text("Thu ng\u00e2n").FontSize(9).Bold();
                right.Item().Height(25); // Space for signature
                if (!string.IsNullOrWhiteSpace(_data.CashierName))
                    right.Item().AlignCenter().Text(_data.CashierName).FontSize(9);
            });
        });
    }
}
