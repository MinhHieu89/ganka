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
/// Creates DrugCatalogItem entities for each valid row and persists them.
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
