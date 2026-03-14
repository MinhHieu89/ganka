using MiniExcelLibs;
using Pharmacy.Application.Features.StockImport;
using Shared.Domain;

namespace Pharmacy.Application.Features.DrugCatalog;

/// <summary>
/// Expected Excel row mapping for drug catalog import template.
/// Columns must match these property names exactly (case-insensitive in MiniExcel).
/// </summary>
public sealed class DrugCatalogExcelRow
{
    public string Name { get; set; } = string.Empty;
    public string NameVi { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string Unit { get; set; } = string.Empty;
    public object? SellingPrice { get; set; }
    public object? MinStockLevel { get; set; }
}

/// <summary>
/// Represents a validated drug catalog row ready for confirmation.
/// </summary>
public sealed record ValidDrugCatalogRow(
    string Name,
    string NameVi,
    string GenericName,
    string Form,
    string Route,
    string? Strength,
    string Unit,
    decimal SellingPrice,
    int MinStockLevel);

/// <summary>
/// Preview result returned after parsing a drug catalog Excel import file.
/// Contains valid rows (ready for confirmation) and row-level errors (for user review).
/// </summary>
public sealed record DrugCatalogImportPreview(
    List<ValidDrugCatalogRow> ValidRows,
    List<ExcelImportError> Errors);

/// <summary>
/// Command to parse a drug catalog Excel file and return a preview with validated rows and errors.
/// The handler does NOT create catalog items -- it returns a preview for user confirmation.
/// </summary>
public sealed record ImportDrugCatalogFromExcelCommand(
    Stream FileStream,
    string FileName = "");

/// <summary>
/// Wolverine static handler for parsing and validating a drug catalog Excel import file.
/// Follows the same two-phase import pattern as ImportStockFromExcel.
/// </summary>
public static class ImportDrugCatalogFromExcelHandler
{
    public static async Task<Result<DrugCatalogImportPreview>> Handle(
        ImportDrugCatalogFromExcelCommand command,
        CancellationToken ct)
    {
        var validRows = new List<ValidDrugCatalogRow>();
        var errors = new List<ExcelImportError>();

        IEnumerable<DrugCatalogExcelRow> rows;
        try
        {
            var excelType = ResolveExcelType(command.FileName);
            Stream parseStream = command.FileStream;

            // Strip BOM for CSV files
            if (excelType == ExcelType.CSV)
            {
                var ms = new MemoryStream();
                await command.FileStream.CopyToAsync(ms, ct);
                var bytes = ms.ToArray();
                var offset = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
                parseStream = new MemoryStream(bytes, offset, bytes.Length - offset);
            }

            rows = MiniExcel.Query<DrugCatalogExcelRow>(parseStream, hasHeader: true, excelType: excelType).ToList();
        }
        catch (Exception ex)
        {
            return Result.Failure<DrugCatalogImportPreview>(
                Error.Validation($"Failed to parse Excel file: {ex.Message}"));
        }

        var rowList = rows.ToList();
        if (rowList.Count == 0)
        {
            return Result.Failure<DrugCatalogImportPreview>(
                Error.Validation("Excel file contains no data rows."));
        }

        for (var i = 0; i < rowList.Count; i++)
        {
            var row = rowList[i];
            var rowNumber = i + 2; // Row 1 is header; data starts at row 2
            var rowErrors = new List<ExcelImportError>();

            // Validate Name (required)
            if (string.IsNullOrWhiteSpace(row.Name))
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "Name", "Name is required."));
            }

            // Validate Unit (required)
            if (string.IsNullOrWhiteSpace(row.Unit))
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "Unit", "Unit is required."));
            }

            // Validate SellingPrice (must be non-negative if provided)
            decimal sellingPrice = 0;
            if (row.SellingPrice is not null)
            {
                if (!TryParseDecimal(row.SellingPrice, out sellingPrice) || sellingPrice < 0)
                {
                    rowErrors.Add(new ExcelImportError(rowNumber, "SellingPrice",
                        "SellingPrice must be a positive number."));
                }
            }

            // Validate MinStockLevel (must be non-negative if provided)
            int minStockLevel = 0;
            if (row.MinStockLevel is not null)
            {
                if (!TryParseInt(row.MinStockLevel, out minStockLevel) || minStockLevel < 0)
                {
                    rowErrors.Add(new ExcelImportError(rowNumber, "MinStockLevel",
                        "MinStockLevel must be a non-negative whole number."));
                }
            }

            if (rowErrors.Count > 0)
            {
                errors.AddRange(rowErrors);
                continue;
            }

            validRows.Add(new ValidDrugCatalogRow(
                Name: row.Name.Trim(),
                NameVi: (row.NameVi ?? string.Empty).Trim(),
                GenericName: (row.GenericName ?? string.Empty).Trim(),
                Form: (row.Form ?? string.Empty).Trim(),
                Route: (row.Route ?? string.Empty).Trim(),
                Strength: row.Strength?.Trim(),
                Unit: row.Unit.Trim(),
                SellingPrice: sellingPrice,
                MinStockLevel: minStockLevel));
        }

        return Result.Success(new DrugCatalogImportPreview(validRows, errors));
    }

    private static bool TryParseInt(object value, out int result)
    {
        result = 0;
        if (value is int i) { result = i; return true; }
        if (value is long l) { result = (int)l; return true; }
        if (value is double d) { result = (int)d; return true; }
        return int.TryParse(value.ToString(), out result);
    }

    private static bool TryParseDecimal(object value, out decimal result)
    {
        result = 0;
        if (value is decimal dec) { result = dec; return true; }
        if (value is double d) { result = (decimal)d; return true; }
        if (value is int i) { result = i; return true; }
        if (value is long l) { result = l; return true; }
        return decimal.TryParse(value.ToString(), out result);
    }

    private static ExcelType ResolveExcelType(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".csv" => ExcelType.CSV,
            ".xlsx" => ExcelType.XLSX,
            ".xls" => ExcelType.XLSX,
            _ => ExcelType.XLSX,
        };
    }
}
