using MiniExcelLibs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;

namespace Pharmacy.Application.Features.StockImport;

/// <summary>
/// Expected Excel row mapping for stock import template.
/// Columns must match these property names exactly (case-insensitive in MiniExcel).
/// Template format: DrugName, BatchNumber, ExpiryDate, Quantity, PurchasePrice
/// </summary>
public sealed class StockImportRow
{
    public string DrugName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public object? Quantity { get; set; }
    public object? PurchasePrice { get; set; }
}

/// <summary>
/// Preview result returned after parsing an Excel import file.
/// Contains valid lines (ready for confirmation) and row-level errors (for user review).
/// The user must confirm before the valid lines are actually saved to stock.
/// </summary>
public sealed record ExcelImportPreview(
    List<StockImportLineDto> ValidLines,
    List<ExcelImportError> Errors);

/// <summary>
/// Represents a validation error on a specific row and column in the Excel import.
/// </summary>
public sealed record ExcelImportError(
    int RowNumber,
    string ColumnName,
    string Message);

/// <summary>
/// Command to parse an Excel file and return a preview with validated lines and row-level errors.
/// PHR-02: Excel bulk import for large orders and initial stock load.
/// The handler does NOT create batches -- it returns a preview for user confirmation.
/// Use CreateStockImportCommand after confirmation to persist the stock.
/// </summary>
public sealed record ImportStockFromExcelCommand(
    Stream FileStream,
    Guid SupplierId);

/// <summary>
/// Wolverine static handler for parsing and validating an Excel stock import file.
/// Uses MiniExcel streaming API for low-memory processing of large files.
/// Validates ALL rows without fail-fast: returns valid lines AND all errors together.
/// </summary>
public static class ImportStockFromExcelHandler
{
    public static async Task<Result<ExcelImportPreview>> Handle(
        ImportStockFromExcelCommand command,
        IDrugCatalogItemRepository drugCatalogItemRepository,
        CancellationToken ct)
    {
        var validLines = new List<StockImportLineDto>();
        var errors = new List<ExcelImportError>();

        IEnumerable<StockImportRow> rows;
        try
        {
            rows = MiniExcel.Query<StockImportRow>(command.FileStream, hasHeader: true).ToList();
        }
        catch (Exception ex)
        {
            return Result.Failure<ExcelImportPreview>(
                Error.Validation($"Failed to parse Excel file: {ex.Message}"));
        }

        var rowList = rows.ToList();
        if (rowList.Count == 0)
        {
            return Result.Failure<ExcelImportPreview>(
                Error.Validation("Excel file contains no data rows."));
        }

        for (var i = 0; i < rowList.Count; i++)
        {
            var row = rowList[i];
            var rowNumber = i + 2; // Row 1 is the header; data starts at row 2
            var rowErrors = new List<ExcelImportError>();

            // Validate DrugName
            if (string.IsNullOrWhiteSpace(row.DrugName))
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "DrugName", "Drug name is required."));
            }

            // Validate BatchNumber
            if (string.IsNullOrWhiteSpace(row.BatchNumber))
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "BatchNumber", "Batch number is required."));
            }

            // Validate ExpiryDate
            DateOnly expiryDate = default;
            if (string.IsNullOrWhiteSpace(row.ExpiryDate))
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "ExpiryDate", "Expiry date is required."));
            }
            else if (!TryParseDate(row.ExpiryDate, out expiryDate))
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "ExpiryDate",
                    "Expiry date must be a valid date in yyyy-MM-dd format."));
            }
            else if (expiryDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "ExpiryDate",
                    "Expiry date must be in the future."));
            }

            // Validate Quantity
            int quantity = 0;
            if (row.Quantity is null)
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "Quantity", "Quantity is required."));
            }
            else if (!TryParseInt(row.Quantity, out quantity) || quantity <= 0)
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "Quantity",
                    "Quantity must be a positive whole number."));
            }

            // Validate PurchasePrice
            decimal purchasePrice = 0;
            if (row.PurchasePrice is null)
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "PurchasePrice", "Purchase price is required."));
            }
            else if (!TryParseDecimal(row.PurchasePrice, out purchasePrice) || purchasePrice < 0)
            {
                rowErrors.Add(new ExcelImportError(rowNumber, "PurchasePrice",
                    "Purchase price must be a non-negative number."));
            }

            if (rowErrors.Count > 0)
            {
                errors.AddRange(rowErrors);
                continue; // Skip to next row -- collect all errors
            }

            // Try to match the drug name to a catalog item
            Guid drugCatalogItemId = Guid.Empty;
            var matchingDrugs = await drugCatalogItemRepository.SearchAsync(row.DrugName, ct);
            var exactMatch = matchingDrugs.FirstOrDefault(d =>
                string.Equals(d.Name, row.DrugName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(d.NameVi, row.DrugName, StringComparison.OrdinalIgnoreCase));

            if (exactMatch is null)
            {
                errors.Add(new ExcelImportError(rowNumber, "DrugName",
                    $"Drug '{row.DrugName}' not found in catalog. Please verify the drug name."));
                continue;
            }

            drugCatalogItemId = exactMatch.Id;

            validLines.Add(new StockImportLineDto(
                Id: Guid.Empty,  // No ID yet -- preview only
                DrugCatalogItemId: drugCatalogItemId,
                DrugName: row.DrugName,
                BatchNumber: row.BatchNumber,
                ExpiryDate: expiryDate,
                Quantity: quantity,
                PurchasePrice: purchasePrice));
        }

        return Result.Success(new ExcelImportPreview(validLines, errors));
    }

    /// <summary>Attempts to parse a date string in common formats (yyyy-MM-dd, dd/MM/yyyy, MM/dd/yyyy).</summary>
    private static bool TryParseDate(string value, out DateOnly result)
    {
        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", out result)) return true;
        if (DateOnly.TryParseExact(value, "dd/MM/yyyy", out result)) return true;
        if (DateOnly.TryParseExact(value, "MM/dd/yyyy", out result)) return true;
        if (DateOnly.TryParse(value, out result)) return true;
        result = default;
        return false;
    }

    /// <summary>Attempts to parse an object (from Excel cell) as an integer.</summary>
    private static bool TryParseInt(object value, out int result)
    {
        result = 0;
        if (value is int i) { result = i; return true; }
        if (value is long l) { result = (int)l; return true; }
        if (value is double d) { result = (int)d; return true; }
        return int.TryParse(value.ToString(), out result);
    }

    /// <summary>Attempts to parse an object (from Excel cell) as a decimal.</summary>
    private static bool TryParseDecimal(object value, out decimal result)
    {
        result = 0;
        if (value is decimal dec) { result = dec; return true; }
        if (value is double d) { result = (decimal)d; return true; }
        if (value is int i) { result = i; return true; }
        if (value is long l) { result = l; return true; }
        return decimal.TryParse(value.ToString(), out result);
    }
}
