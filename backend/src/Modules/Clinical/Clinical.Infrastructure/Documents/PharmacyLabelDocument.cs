using Clinical.Infrastructure.Documents.Shared;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Clinical.Infrastructure.Documents;

/// <summary>
/// QuestPDF document for pharmacy label.
/// Small label format (~70 x 35mm) for adhesive label printers.
/// Compact layout with drug name, dosage, and patient info.
/// </summary>
public sealed class PharmacyLabelDocument : IDocument
{
    private readonly PharmacyLabelData _data;

    public PharmacyLabelDocument(PharmacyLabelData data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(70, 35, Unit.Millimetre);
            page.Margin(3, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Noto Sans"));

            page.Content().Column(col =>
            {
                // Clinic name (abbreviated if needed)
                var clinicDisplay = _data.ClinicName.Length > 30
                    ? _data.ClinicName[..30] + "..."
                    : _data.ClinicName;
                col.Item().Text(clinicDisplay).FontSize(6).Bold();

                // Patient name
                col.Item().Text($"BN: {_data.PatientName}").FontSize(7);

                // Drug name + strength
                var drugDisplay = _data.DrugName;
                if (!string.IsNullOrWhiteSpace(_data.Strength))
                    drugDisplay += $" {_data.Strength}";
                col.Item().Text(drugDisplay).FontSize(8).Bold();

                // Dosage instructions
                var dosageDisplay = !string.IsNullOrWhiteSpace(_data.DosageOverride)
                    ? _data.DosageOverride
                    : _data.Dosage ?? "";
                if (!string.IsNullOrWhiteSpace(dosageDisplay))
                    col.Item().Text(dosageDisplay).FontSize(7);

                // Quantity + Date
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"SL: {_data.Quantity} {_data.Unit}").FontSize(6);
                    row.RelativeItem().AlignRight().Text(_data.DispensedDate.ToString("dd/MM/yyyy")).FontSize(6);
                });
            });
        });
    }
}
