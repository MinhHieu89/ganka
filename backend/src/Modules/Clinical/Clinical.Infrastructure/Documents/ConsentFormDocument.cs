using Clinical.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinical.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for consent form (Phieu dong y thu thuat / dieu tri).
/// A4 paper with procedure description, patient signature, and witness lines.
/// </summary>
public sealed class ConsentFormDocument : IDocument
{
    private readonly ConsentFormData _data;
    private readonly ClinicHeaderData _header;

    public ConsentFormDocument(ConsentFormData data, ClinicHeaderData header)
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
            page.Margin(20, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Noto Sans"));

            page.Header().Component(new ClinicHeaderComponent(_header));

            page.Content().PaddingTop(15).Column(col =>
            {
                // Title
                col.Item().AlignCenter().Text("PHI\u1ebeU \u0110\u1ed2NG \u00dd TH\u1ee6 THU\u1eacT / \u0110I\u1ec0U TR\u1eca").FontSize(14).Bold();
                col.Item().Height(15);

                // Patient info
                col.Item().Text(text =>
                {
                    text.Span("H\u1ecd t\u00ean b\u1ec7nh nh\u00e2n: ").Bold();
                    text.Span(_data.PatientName);
                });

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text(text =>
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

                col.Item().Height(10);

                // Diagnosis
                if (_data.Diagnoses.Count > 0)
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Ch\u1ea9n \u0111o\u00e1n: ").Bold();
                        text.Span(string.Join(", ", _data.Diagnoses));
                    });
                    col.Item().Height(10);
                }

                // Procedure
                col.Item().Text(text =>
                {
                    text.Span("Th\u1ee7 thu\u1eadt / \u0110i\u1ec1u tr\u1ecb: ").Bold();
                    text.Span(_data.ProcedureType);
                });

                col.Item().Height(15);

                // Explanation section
                col.Item().Text("B\u00e1c s\u0129 \u0111\u00e3 gi\u1ea3i th\u00edch cho t\u00f4i v\u1ec1:").Bold();
                col.Item().Height(5);
                col.Item().PaddingLeft(10).Column(list =>
                {
                    list.Item().Text("\u2022 M\u1ee5c \u0111\u00edch c\u1ee7a th\u1ee7 thu\u1eadt / \u0111i\u1ec1u tr\u1ecb");
                    list.Item().Text("\u2022 C\u00e1c nguy c\u01a1 v\u00e0 bi\u1ebfn ch\u1ee9ng c\u00f3 th\u1ec3 x\u1ea3y ra");
                    list.Item().Text("\u2022 C\u00e1c ph\u01b0\u01a1ng ph\u00e1p \u0111i\u1ec1u tr\u1ecb thay th\u1ebf (n\u1ebfu c\u00f3)");
                    list.Item().Text("\u2022 K\u1ebft qu\u1ea3 d\u1ef1 ki\u1ebfn v\u00e0 h\u1eadu ph\u1eabu");
                });

                col.Item().Height(15);

                // Consent statement
                col.Item().Border(0.5f).Padding(10).Column(consent =>
                {
                    consent.Item().Text(
                        "T\u00f4i \u0111\u00e3 \u0111\u01b0\u1ee3c gi\u1ea3i th\u00edch \u0111\u1ea7y \u0111\u1ee7 v\u1ec1 t\u00ecnh tr\u1ea1ng b\u1ec7nh, ph\u01b0\u01a1ng ph\u00e1p \u0111i\u1ec1u tr\u1ecb, " +
                        "c\u00e1c nguy c\u01a1 v\u00e0 l\u1ee3i \u00edch. T\u00f4i \u0111\u1ed3ng \u00fd th\u1ef1c hi\u1ec7n th\u1ee7 thu\u1eadt / \u0111i\u1ec1u tr\u1ecb n\u00eau tr\u00ean."
                    ).Bold();
                });
            });

            page.Footer().Column(col =>
            {
                col.Item().Text($"Ng\u00e0y {_data.FormDate:dd} th\u00e1ng {_data.FormDate:MM} n\u0103m {_data.FormDate:yyyy}").FontSize(9).AlignCenter();
                col.Item().Height(10);

                col.Item().Row(row =>
                {
                    // Patient signature
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().AlignCenter().Text("B\u1ec7nh nh\u00e2n").FontSize(10).Bold();
                        left.Item().AlignCenter().Text("(K\u00fd v\u00e0 ghi r\u00f5 h\u1ecd t\u00ean)").FontSize(8).Italic();
                        left.Item().Height(35);
                        left.Item().AlignCenter().Text(_data.PatientName).FontSize(9);
                    });

                    // Fingerprint space
                    row.ConstantItem(80).Column(mid =>
                    {
                        mid.Item().AlignCenter().Text("\u0110i\u1ec3m ch\u1ec9").FontSize(8).Italic();
                        mid.Item().Height(40);
                    });

                    // Doctor / witness signature
                    row.RelativeItem().Column(right =>
                    {
                        right.Item().AlignCenter().Text("B\u00e1c s\u0129 / Nh\u00e2n ch\u1ee9ng").FontSize(10).Bold();
                        right.Item().AlignCenter().Text("(K\u00fd v\u00e0 ghi r\u00f5 h\u1ecd t\u00ean)").FontSize(8).Italic();
                        right.Item().Height(35);
                        right.Item().AlignCenter().Text(_data.DoctorName).FontSize(9);
                    });
                });
            });
        });
    }
}
