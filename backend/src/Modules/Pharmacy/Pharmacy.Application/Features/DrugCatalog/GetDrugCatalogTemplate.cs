using MiniExcelLibs;

namespace Pharmacy.Application.Features.DrugCatalog;

/// <summary>
/// Query to retrieve a blank drug catalog Excel import template.
/// </summary>
public sealed record GetDrugCatalogTemplateQuery;

/// <summary>
/// Wolverine static handler that returns a drug catalog Excel template.
/// The template contains column headers matching DrugCatalogExcelRow properties.
/// </summary>
public static class GetDrugCatalogTemplateHandler
{
    private static readonly List<Dictionary<string, object>> TemplateColumns = new()
    {
        new Dictionary<string, object>
        {
            ["Name"] = "",
            ["NameVi"] = "",
            ["GenericName"] = "",
            ["Form"] = "",
            ["Route"] = "",
            ["Strength"] = "",
            ["Unit"] = "",
            ["SellingPrice"] = "",
            ["MinStockLevel"] = ""
        }
    };

    public static byte[] Handle(GetDrugCatalogTemplateQuery query)
    {
        using var stream = new MemoryStream();
        MiniExcel.SaveAs(stream, TemplateColumns);
        return stream.ToArray();
    }
}
