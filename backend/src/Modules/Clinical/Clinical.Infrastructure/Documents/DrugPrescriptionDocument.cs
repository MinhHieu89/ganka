using Clinical.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinical.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for drug prescription (Don thuoc).
/// A5 paper size (148 x 210mm) per Vietnamese MOH convention.
/// Includes clinic header, patient info, diagnosis, drug table, Loi dan, and doctor signature.
/// </summary>
public sealed class DrugPrescriptionDocument : IDocument
{
    private readonly DrugPrescriptionData _data;
    private readonly ClinicHeaderData _header;

    public DrugPrescriptionDocument(DrugPrescriptionData data, ClinicHeaderData header)
    {
        _data = data;
        _header = header;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(148, 210, Unit.Millimetre);
            page.Margin(8, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Noto Sans"));

            page.Header().Component(new ClinicHeaderComponent(_header));

            page.Content().PaddingTop(5).Column(col =>
            {
                // Title
                col.Item().AlignCenter().Text("\u0110\u01a0N THU\u1ed0C").FontSize(12).Bold();
                col.Item().Height(5);

                // Prescription code
                if (!string.IsNullOrWhiteSpace(_data.PrescriptionCode))
                    col.Item().Text($"S\u1ed1: {_data.PrescriptionCode}").FontSize(8);

                // Patient info
                col.Item().Row(row =>
                {
                    row.RelativeItem(2).Text(text =>
                    {
                        text.Span("H\u1ecd t\u00ean: ").Bold();
                        text.Span(_data.PatientName);
                    });
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("M\u00e3 BN: ").Bold();
                        text.Span(_data.PatientCode ?? "");
                    });
                });

                col.Item().Row(row =>
                {
                    row.RelativeItem(2).Text(text =>
                    {
                        text.Span("Ng\u00e0y sinh: ").Bold();
                        text.Span(_data.DateOfBirth?.ToString("dd/MM/yyyy") ?? "");
                    });
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("Gi\u1edbi t\u00ednh: ").Bold();
                        text.Span(_data.Gender ?? "");
                    });
                });

                if (!string.IsNullOrWhiteSpace(_data.Address))
                {
                    col.Item().Text(text =>
                    {
                        text.Span("\u0110\u1ecba ch\u1ec9: ").Bold();
                        text.Span(_data.Address);
                    });
                }

                if (!string.IsNullOrWhiteSpace(_data.IdentityNumber))
                {
                    col.Item().Text(text =>
                    {
                        text.Span("CCCD: ").Bold();
                        text.Span(_data.IdentityNumber);
                    });
                }

                col.Item().Height(3);

                // Diagnosis
                col.Item().Text(text =>
                {
                    text.Span("Ch\u1ea9n \u0111o\u00e1n: ").Bold();
                    text.Span(string.Join(", ", _data.Diagnoses));
                });

                col.Item().Height(5);

                // Drug table
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(20); // STT
                        columns.RelativeColumn(3);   // Ten thuoc
                        columns.RelativeColumn(2);   // Cach dung
                        columns.ConstantColumn(25);  // SL
                        columns.ConstantColumn(35);  // Don vi
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Border(0.5f).Padding(2).Text("STT").FontSize(8).Bold().AlignCenter();
                        header.Cell().Border(0.5f).Padding(2).Text("T\u00ean thu\u1ed1c").FontSize(8).Bold();
                        header.Cell().Border(0.5f).Padding(2).Text("C\u00e1ch d\u00f9ng").FontSize(8).Bold();
                        header.Cell().Border(0.5f).Padding(2).Text("SL").FontSize(8).Bold().AlignCenter();
                        header.Cell().Border(0.5f).Padding(2).Text("\u0110VT").FontSize(8).Bold().AlignCenter();
                    });

                    // Data rows
                    foreach (var item in _data.Items.OrderBy(i => i.SortOrder))
                    {
                        var drugDisplay = item.DrugName;
                        if (!string.IsNullOrWhiteSpace(item.GenericName))
                            drugDisplay += $"\n({item.GenericName})";
                        if (!string.IsNullOrWhiteSpace(item.Strength))
                            drugDisplay += $" {item.Strength}";

                        var dosageDisplay = !string.IsNullOrWhiteSpace(item.DosageOverride)
                            ? item.DosageOverride
                            : item.Dosage ?? "";

                        table.Cell().Border(0.5f).Padding(2).Text(item.SortOrder.ToString()).FontSize(8).AlignCenter();
                        table.Cell().Border(0.5f).Padding(2).Text(drugDisplay).FontSize(8);
                        table.Cell().Border(0.5f).Padding(2).Text(dosageDisplay).FontSize(8);
                        table.Cell().Border(0.5f).Padding(2).Text(item.Quantity.ToString()).FontSize(8).AlignCenter();
                        table.Cell().Border(0.5f).Padding(2).Text(item.Unit).FontSize(8).AlignCenter();
                    }
                });

                col.Item().Height(5);

                // Loi dan (Doctor's advice)
                if (!string.IsNullOrWhiteSpace(_data.Notes))
                {
                    col.Item().Text(text =>
                    {
                        text.Span("L\u1eddi d\u1eb7n: ").Bold();
                        text.Span(_data.Notes);
                    });
                }
            });

            page.Footer().Column(col =>
            {
                col.Item().AlignRight().Column(right =>
                {
                    right.Item().AlignCenter().Text($"Ng\u00e0y {_data.PrescribedAt:dd} th\u00e1ng {_data.PrescribedAt:MM} n\u0103m {_data.PrescribedAt:yyyy}").FontSize(8);
                    right.Item().AlignCenter().Text("B\u00e1c s\u0129 kh\u00e1m b\u1ec7nh").FontSize(9).Bold();
                    right.Item().Height(30); // Space for signature
                    right.Item().AlignCenter().Text(_data.DoctorName).FontSize(9);
                });
            });
        });
    }
}
