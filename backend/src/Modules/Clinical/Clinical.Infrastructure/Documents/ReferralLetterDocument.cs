using Clinical.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinical.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for referral letter (Giay chuyen vien).
/// A4 paper with patient info, diagnosis, and referral reason.
/// </summary>
public sealed class ReferralLetterDocument : IDocument
{
    private readonly ReferralLetterData _data;
    private readonly ClinicHeaderData _header;

    public ReferralLetterDocument(ReferralLetterData data, ClinicHeaderData header)
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
                col.Item().AlignCenter().Text("GI\u1ea4Y CHUY\u1ec2N VI\u1ec6N").FontSize(16).Bold();
                col.Item().Height(15);

                // To
                col.Item().Text(text =>
                {
                    text.Span("K\u00ednh g\u1eedi: ").Bold();
                    text.Span(_data.ReferralTo);
                });

                col.Item().Height(10);

                // Patient info block
                col.Item().Text("TH\u00d4NG TIN B\u1ec6NH NH\u00c2N").FontSize(11).Bold().Underline();
                col.Item().Height(5);

                col.Item().Text(text =>
                {
                    text.Span("H\u1ecd t\u00ean: ").Bold();
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
                col.Item().Text(text =>
                {
                    text.Span("Ch\u1ea9n \u0111o\u00e1n: ").Bold();
                    text.Span(string.Join(", ", _data.Diagnoses));
                });

                col.Item().Height(10);

                // Reason for referral
                col.Item().Text("L\u00dd DO CHUY\u1ec2N VI\u1ec6N").FontSize(11).Bold().Underline();
                col.Item().Height(5);
                col.Item().Text(_data.ReferralReason);

                col.Item().Height(10);

                // Clinical summary
                if (!string.IsNullOrWhiteSpace(_data.ExaminationNotes))
                {
                    col.Item().Text("T\u00d3M T\u1eaeT B\u1ec6NH \u00c1N").FontSize(11).Bold().Underline();
                    col.Item().Height(5);
                    col.Item().Text(_data.ExaminationNotes);
                }
            });

            page.Footer().Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Space for receiving hospital stamp
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().AlignCenter().Text("N\u01a1i ti\u1ebfp nh\u1eadn").FontSize(9).Bold();
                        left.Item().Height(40);
                        left.Item().AlignCenter().Text("(K\u00fd, \u0111\u00f3ng d\u1ea5u)").FontSize(8).Italic();
                    });

                    // Referring doctor signature
                    row.RelativeItem().Column(right =>
                    {
                        right.Item().AlignCenter().Text($"Ng\u00e0y {_data.ReferralDate:dd} th\u00e1ng {_data.ReferralDate:MM} n\u0103m {_data.ReferralDate:yyyy}").FontSize(9);
                        right.Item().AlignCenter().Text("B\u00e1c s\u0129 chuy\u1ec3n vi\u1ec7n").FontSize(10).Bold();
                        right.Item().Height(35);
                        right.Item().AlignCenter().Text(_data.DoctorName).FontSize(10);
                    });
                });
            });
        });
    }
}
