using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Billing.Infrastructure.Documents.Shared;

/// <summary>
/// Reusable QuestPDF component that renders the clinic header block.
/// Mirrors the Clinical module ClinicHeaderComponent for consistent branding
/// across billing documents (invoice, receipt, e-invoice, shift report).
/// </summary>
public sealed class ClinicHeaderComponent : IComponent
{
    private readonly ClinicHeaderData _data;

    public ClinicHeaderComponent(ClinicHeaderData data)
    {
        _data = data;
    }

    public void Compose(IContainer container)
    {
        container.Row(row =>
        {
            if (_data.LogoBytes is { Length: > 0 })
            {
                row.ConstantItem(60).Height(60).Image(_data.LogoBytes);
                row.ConstantItem(10);
            }

            row.RelativeItem().Column(col =>
            {
                var displayName = _data.ClinicNameVi ?? _data.ClinicName;
                col.Item().Text(displayName).FontSize(14).Bold();

                if (!string.IsNullOrWhiteSpace(_data.Address))
                    col.Item().Text($"\u0110\u1ecba ch\u1ec9: {_data.Address}").FontSize(8);

                var contactParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(_data.Phone))
                    contactParts.Add($"\u0110T: {_data.Phone}");
                if (!string.IsNullOrWhiteSpace(_data.Fax))
                    contactParts.Add($"Fax: {_data.Fax}");
                if (contactParts.Count > 0)
                    col.Item().Text(string.Join(" | ", contactParts)).FontSize(8);

                if (!string.IsNullOrWhiteSpace(_data.TaxCode))
                    col.Item().Text($"MST: {_data.TaxCode}").FontSize(8);

                if (!string.IsNullOrWhiteSpace(_data.LicenseNumber))
                    col.Item().Text($"GPCN: {_data.LicenseNumber}").FontSize(7).Italic();

                if (!string.IsNullOrWhiteSpace(_data.Tagline))
                    col.Item().Text(_data.Tagline).FontSize(7).Italic();
            });
        });
    }
}
