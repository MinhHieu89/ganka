using Clinical.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Clinical.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for batch pharmacy labels.
/// Generates a multi-page PDF with one 70x35mm label per prescribed drug.
/// Uses the same compact layout as PharmacyLabelDocument but loops over all items.
/// </summary>
public sealed class BatchPharmacyLabelDocument : IDocument
{
    private readonly IReadOnlyList<PharmacyLabelData> _labels;

    public BatchPharmacyLabelDocument(IReadOnlyList<PharmacyLabelData> labels)
    {
        _labels = labels;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        foreach (var label in _labels)
        {
            container.Page(page =>
            {
                page.Size(70, 35, Unit.Millimetre);
                page.Margin(3, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Noto Sans"));

                page.Content().Column(col =>
                {
                    // Clinic name (abbreviated if needed)
                    var clinicDisplay = label.ClinicName.Length > 30
                        ? label.ClinicName[..30] + "..."
                        : label.ClinicName;
                    col.Item().Text(clinicDisplay).FontSize(6).Bold();

                    // Patient name
                    col.Item().Text($"BN: {label.PatientName}").FontSize(7);

                    // Drug name + strength
                    var drugDisplay = label.DrugName;
                    if (!string.IsNullOrWhiteSpace(label.Strength))
                        drugDisplay += $" {label.Strength}";
                    col.Item().Text(drugDisplay).FontSize(8).Bold();

                    // Dosage instructions
                    var dosageDisplay = !string.IsNullOrWhiteSpace(label.DosageOverride)
                        ? label.DosageOverride
                        : label.Dosage ?? "";
                    if (!string.IsNullOrWhiteSpace(dosageDisplay))
                        col.Item().Text(dosageDisplay).FontSize(7);

                    // Quantity + Date
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"SL: {label.Quantity} {label.Unit}").FontSize(6);
                        row.RelativeItem().AlignRight().Text(label.DispensedDate.ToString("dd/MM/yyyy")).FontSize(6);
                    });
                });
            });
        }
    }
}
