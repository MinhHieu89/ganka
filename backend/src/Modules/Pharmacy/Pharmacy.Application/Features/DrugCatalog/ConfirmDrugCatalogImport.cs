using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Application.Features.DrugCatalog;

/// <summary>
/// Command to confirm and persist validated drug catalog rows from an Excel import preview.
/// Takes the valid rows from ImportDrugCatalogFromExcel and creates DrugCatalogItem entities.
/// </summary>
public sealed record ConfirmDrugCatalogImportCommand(
    List<ValidDrugCatalogRow> ValidRows,
    Guid BranchId);

/// <summary>
/// Wolverine static handler for confirming a drug catalog import.
/// Re-validates all rows server-side, checks for duplicates within batch and against
/// existing catalog, then creates DrugCatalogItem entities for each valid row and persists them.
/// </summary>
public static class ConfirmDrugCatalogImportHandler
{
    public static async Task<Result<int>> Handle(
        ConfirmDrugCatalogImportCommand command,
        IDrugCatalogItemRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (command.ValidRows.Count == 0)
            return Result.Success(0);

        // Server-side re-validation
        var validationErrors = new List<string>();

        foreach (var row in command.ValidRows)
        {
            if (string.IsNullOrWhiteSpace(row.Name))
                validationErrors.Add($"Row with Name '{row.Name}': Name is required.");

            if (string.IsNullOrWhiteSpace(row.Unit))
                validationErrors.Add($"Row '{row.Name}': Unit is required.");

            if (row.SellingPrice < 0)
                validationErrors.Add($"Row '{row.Name}': SellingPrice must be non-negative.");

            if (row.MinStockLevel < 0)
                validationErrors.Add($"Row '{row.Name}': MinStockLevel must be non-negative.");
        }

        if (validationErrors.Count > 0)
        {
            return Result.Failure<int>(
                Error.Validation(string.Join(" ", validationErrors)));
        }

        // Check for duplicate names within batch
        var batchNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var batchDuplicates = new List<string>();
        foreach (var row in command.ValidRows)
        {
            if (!batchNames.Add(row.Name))
                batchDuplicates.Add(row.Name);
        }

        if (batchDuplicates.Count > 0)
        {
            return Result.Failure<int>(
                Error.Validation($"Duplicate drug names within batch: {string.Join(", ", batchDuplicates.Distinct())}"));
        }

        // Check for duplicates against existing catalog
        var existingItems = await repository.GetAllActiveAsync(ct);
        var existingNames = new HashSet<string>(
            existingItems.Select(i => i.Name),
            StringComparer.OrdinalIgnoreCase);

        var catalogDuplicates = command.ValidRows
            .Where(r => existingNames.Contains(r.Name))
            .Select(r => r.Name)
            .Distinct()
            .ToList();

        if (catalogDuplicates.Count > 0)
        {
            return Result.Failure<int>(
                Error.Validation($"Drug names already exist in catalog: {string.Join(", ", catalogDuplicates)}"));
        }

        foreach (var row in command.ValidRows)
        {
            var form = ParseEnum<DrugForm>(row.Form);
            var route = ParseEnum<DrugRoute>(row.Route);

            var item = DrugCatalogItem.Create(
                name: row.Name,
                nameVi: row.NameVi,
                genericName: row.GenericName,
                form: form,
                strength: row.Strength,
                route: route,
                unit: row.Unit,
                defaultDosageTemplate: null,
                branchId: new BranchId(command.BranchId));

            if (row.SellingPrice > 0 || row.MinStockLevel > 0)
            {
                item.UpdatePricing(row.SellingPrice > 0 ? row.SellingPrice : null, row.MinStockLevel);
            }

            repository.Add(item);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(command.ValidRows.Count);
    }

    private static T ParseEnum<T>(string value) where T : struct, Enum
    {
        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
            return result;
        return default;
    }
}
