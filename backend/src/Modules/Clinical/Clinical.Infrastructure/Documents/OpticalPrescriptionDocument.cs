using Clinical.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinical.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for optical prescription (Don kinh).
/// A4 paper with OD/OS refraction grid and lens type recommendation.
/// </summary>
public sealed class OpticalPrescriptionDocument : IDocument
{
    private readonly OpticalPrescriptionData _data;
    private readonly ClinicHeaderData _header;

    public OpticalPrescriptionDocument(OpticalPrescriptionData data, ClinicHeaderData header)
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

            page.Content().PaddingTop(10).Column(col =>
            {
                // Title
                col.Item().AlignCenter().Text("\u0110\u01a0N K\u00cdNH").FontSize(14).Bold();
                col.Item().Height(10);

                // Patient info
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

                col.Item().Height(10);

                // Distance Rx table
                col.Item().Text("K\u00cdNH NH\u00ccN XA").FontSize(11).Bold();
                col.Item().Height(5);
                ComposeRefractionTable(col,
                    _data.OdSph, _data.OdCyl, _data.OdAxis, _data.OdAdd,
                    _data.OsSph, _data.OsCyl, _data.OsAxis, _data.OsAdd);

                // Near Rx table (only if near overrides present)
                if (_data.NearOdSph.HasValue || _data.NearOsSph.HasValue)
                {
                    col.Item().Height(10);
                    col.Item().Text("K\u00cdNH NH\u00ccN G\u1ea6N").FontSize(11).Bold();
                    col.Item().Height(5);
                    ComposeRefractionTable(col,
                        _data.NearOdSph, _data.NearOdCyl, _data.NearOdAxis, null,
                        _data.NearOsSph, _data.NearOsCyl, _data.NearOsAxis, null);
                }

                col.Item().Height(10);

                // PD
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("PD xa: ").Bold();
                        text.Span(_data.FarPd?.ToString("0.0") ?? "___");
                        text.Span(" mm");
                    });
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("PD g\u1ea7n: ").Bold();
                        text.Span(_data.NearPd?.ToString("0.0") ?? "___");
                        text.Span(" mm");
                    });
                });

                col.Item().Height(5);

                // Lens type
                if (!string.IsNullOrWhiteSpace(_data.LensType))
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Lo\u1ea1i tr\u00f2ng k\u00ednh: ").Bold();
                        text.Span(_data.LensType);
                    });
                }

                // Notes
                if (!string.IsNullOrWhiteSpace(_data.Notes))
                {
                    col.Item().Height(5);
                    col.Item().Text(text =>
                    {
                        text.Span("Ghi ch\u00fa: ").Bold();
                        text.Span(_data.Notes);
                    });
                }
            });

            page.Footer().AlignRight().Column(col =>
            {
                col.Item().AlignCenter().Text($"Ng\u00e0y {_data.PrescribedAt:dd} th\u00e1ng {_data.PrescribedAt:MM} n\u0103m {_data.PrescribedAt:yyyy}").FontSize(9);
                col.Item().AlignCenter().Text("B\u00e1c s\u0129 kh\u00e1m b\u1ec7nh").FontSize(10).Bold();
                col.Item().Height(35);
                col.Item().AlignCenter().Text(_data.DoctorName).FontSize(10);
            });
        });
    }

    private static void ComposeRefractionTable(ColumnDescriptor col,
        decimal? odSph, decimal? odCyl, int? odAxis, decimal? odAdd,
        decimal? osSph, decimal? osCyl, int? osAxis, decimal? osAdd)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40); // Eye
                columns.RelativeColumn();    // SPH
                columns.RelativeColumn();    // CYL
                columns.RelativeColumn();    // AXIS
                columns.RelativeColumn();    // ADD
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Border(0.5f).Padding(3).Text("M\u1eaft").FontSize(9).Bold().AlignCenter();
                header.Cell().Border(0.5f).Padding(3).Text("SPH").FontSize(9).Bold().AlignCenter();
                header.Cell().Border(0.5f).Padding(3).Text("CYL").FontSize(9).Bold().AlignCenter();
                header.Cell().Border(0.5f).Padding(3).Text("AXIS").FontSize(9).Bold().AlignCenter();
                header.Cell().Border(0.5f).Padding(3).Text("ADD").FontSize(9).Bold().AlignCenter();
            });

            // OD row
            table.Cell().Border(0.5f).Padding(3).Text("OD (P)").FontSize(9).Bold().AlignCenter();
            table.Cell().Border(0.5f).Padding(3).Text(FormatDiopter(odSph)).FontSize(9).AlignCenter();
            table.Cell().Border(0.5f).Padding(3).Text(FormatDiopter(odCyl)).FontSize(9).AlignCenter();
            table.Cell().Border(0.5f).Padding(3).Text(odAxis?.ToString("0") ?? "").FontSize(9).AlignCenter();
            table.Cell().Border(0.5f).Padding(3).Text(FormatDiopter(odAdd)).FontSize(9).AlignCenter();

            // OS row
            table.Cell().Border(0.5f).Padding(3).Text("OS (T)").FontSize(9).Bold().AlignCenter();
            table.Cell().Border(0.5f).Padding(3).Text(FormatDiopter(osSph)).FontSize(9).AlignCenter();
            table.Cell().Border(0.5f).Padding(3).Text(FormatDiopter(osCyl)).FontSize(9).AlignCenter();
            table.Cell().Border(0.5f).Padding(3).Text(osAxis?.ToString("0") ?? "").FontSize(9).AlignCenter();
            table.Cell().Border(0.5f).Padding(3).Text(FormatDiopter(osAdd)).FontSize(9).AlignCenter();
        });
    }

    private static string FormatDiopter(decimal? value)
    {
        if (!value.HasValue) return "";
        var v = value.Value;
        return v > 0 ? $"+{v:0.00}" : $"{v:0.00}";
    }
}
