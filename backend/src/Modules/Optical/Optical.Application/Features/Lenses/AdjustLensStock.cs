using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Shared.Domain;

namespace Optical.Application.Features.Lenses;

/// <summary>
/// Command to adjust stock for a specific lens power combination.
/// A positive QuantityChange increases stock; negative decreases stock.
/// </summary>
public sealed record AdjustLensStockCommand(
    Guid LensCatalogItemId,
    decimal Sph,
    decimal Cyl,
    decimal? Add,
    int QuantityChange,
    string Reason,
    int? MinStockLevel = null);

/// <summary>
/// Wolverine static handler for adjusting lens stock entries.
///
/// Logic:
/// 1. Find the LensCatalogItem by ID (return NotFound if missing).
/// 2. Look up the stock entry for the given power combination (SPH/CYL/ADD).
/// 3. If no entry exists and QuantityChange > 0: create a new LensStockEntry via AddStockEntry.
/// 4. If no entry exists and QuantityChange &lt;= 0: return validation error.
/// 5. If entry exists: adjust via AdjustStockEntry (throws on negative stock).
/// 6. Persist and return updated stock entry DTO.
/// </summary>
public static class AdjustLensStockHandler
{
    public static async Task<Result<LensStockEntryDto>> Handle(
        AdjustLensStockCommand command,
        ILensCatalogRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        // Step 1: Load the catalog item
        var item = await repository.GetByIdAsync(command.LensCatalogItemId, ct);
        if (item is null)
            return Result.Failure<LensStockEntryDto>(
                Error.NotFound("LensCatalogItem", command.LensCatalogItemId));

        // Step 2: Look up existing stock entry for this power combination
        var existingEntry = await repository.GetStockEntryAsync(
            command.LensCatalogItemId,
            command.Sph,
            command.Cyl,
            command.Add,
            ct);

        LensStockEntry resultEntry;

        if (existingEntry is null)
        {
            // Step 3: No existing entry — only allow positive adjustments (adding new stock)
            if (command.QuantityChange <= 0)
            {
                return Result.Failure<LensStockEntryDto>(Error.Validation(
                    $"No stock entry exists for SPH={command.Sph}/CYL={command.Cyl}" +
                    $"{(command.Add.HasValue ? $"/ADD={command.Add}" : "")}. " +
                    "Cannot deduct from non-existent stock."));
            }

            // Create a new stock entry for this power combination
            resultEntry = item.AddStockEntry(
                sph: command.Sph,
                cyl: command.Cyl,
                add: command.Add,
                quantity: command.QuantityChange);
        }
        else
        {
            // Step 5: Existing entry — adjust stock (throws InvalidOperationException if negative)
            try
            {
                item.AdjustStockEntry(existingEntry.Id, command.QuantityChange);
                resultEntry = existingEntry;
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<LensStockEntryDto>(Error.Validation(ex.Message));
            }
        }

        if (command.MinStockLevel.HasValue)
            resultEntry.UpdateMinStockLevel(command.MinStockLevel.Value);

        await unitOfWork.SaveChangesAsync(ct);

        return new LensStockEntryDto(
            Id: resultEntry.Id,
            LensCatalogItemId: resultEntry.LensCatalogItemId,
            Sph: resultEntry.Sph,
            Cyl: resultEntry.Cyl,
            Add: resultEntry.Add,
            Quantity: resultEntry.Quantity,
            MinStockLevel: resultEntry.MinStockLevel);
    }
}
