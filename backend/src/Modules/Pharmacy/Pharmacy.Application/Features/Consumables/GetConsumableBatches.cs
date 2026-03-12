using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;

namespace Pharmacy.Application.Features.Consumables;

/// <summary>
/// Query to retrieve all batches for a specific consumable item (ExpiryTracked mode).
/// Returns batches ordered by expiry date (FEFO order) for inventory inspection.
/// </summary>
public sealed record GetConsumableBatchesQuery(Guid ConsumableItemId);

/// <summary>
/// Wolverine static handler for retrieving consumable batches.
/// Returns all batches ordered by ExpiryDate ascending (FEFO).
/// </summary>
public static class GetConsumableBatchesHandler
{
    public static async Task<Result<List<ConsumableBatchDto>>> Handle(
        GetConsumableBatchesQuery query,
        IConsumableRepository repository,
        CancellationToken ct)
    {
        var batches = await repository.GetBatchesAsync(query.ConsumableItemId, ct);

        var dtos = batches
            .OrderBy(b => b.ExpiryDate)
            .Select(b => new ConsumableBatchDto(
                Id: b.Id,
                ConsumableItemId: b.ConsumableItemId,
                BatchNumber: b.BatchNumber,
                ExpiryDate: b.ExpiryDate,
                InitialQuantity: b.InitialQuantity,
                CurrentQuantity: b.CurrentQuantity,
                IsExpired: b.IsExpired,
                IsNearExpiry: b.IsNearExpiry(30)))
            .ToList();

        return Result<List<ConsumableBatchDto>>.Success(dtos);
    }
}
