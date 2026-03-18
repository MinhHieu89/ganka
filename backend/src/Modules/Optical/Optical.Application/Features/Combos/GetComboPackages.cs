using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Combos;

/// <summary>
/// Query to retrieve all combo packages with optional inactive filter.
/// </summary>
public sealed record GetComboPackagesQuery(bool IncludeInactive = false);

/// <summary>
/// Wolverine static handler for retrieving combo packages.
/// Loads frame and lens names for display by looking up FK references if set.
/// </summary>
public static class GetComboPackagesHandler
{
    public static async Task<Result<List<ComboPackageDto>>> Handle(
        GetComboPackagesQuery query,
        IComboPackageRepository repository,
        IFrameRepository frameRepository,
        ILensCatalogRepository lensRepository,
        CancellationToken ct)
    {
        var combos = await repository.GetAllAsync(query.IncludeInactive, ct);

        // Batch-load all referenced frames and lenses to avoid N+1 queries
        var frameIds = combos.Where(c => c.FrameId.HasValue).Select(c => c.FrameId!.Value).Distinct().ToList();
        var lensIds = combos.Where(c => c.LensCatalogItemId.HasValue).Select(c => c.LensCatalogItemId!.Value).Distinct().ToList();

        var frameLookup = new Dictionary<Guid, string>();
        foreach (var frameId in frameIds)
        {
            var frame = await frameRepository.GetByIdAsync(frameId, ct);
            if (frame is not null)
                frameLookup[frameId] = $"{frame.Brand} {frame.Model}";
        }

        var lensLookup = new Dictionary<Guid, string>();
        foreach (var lensId in lensIds)
        {
            var lens = await lensRepository.GetByIdAsync(lensId, ct);
            if (lens is not null)
                lensLookup[lensId] = $"{lens.Brand} {lens.Name}";
        }

        var result = new List<ComboPackageDto>(combos.Count);

        foreach (var combo in combos)
        {
            string? frameName = combo.FrameId.HasValue && frameLookup.TryGetValue(combo.FrameId.Value, out var fn) ? fn : null;
            string? lensName = combo.LensCatalogItemId.HasValue && lensLookup.TryGetValue(combo.LensCatalogItemId.Value, out var ln) ? ln : null;

            result.Add(new ComboPackageDto(
                Id: combo.Id,
                Name: combo.Name,
                Description: combo.Description,
                FrameId: combo.FrameId,
                FrameName: frameName,
                LensCatalogItemId: combo.LensCatalogItemId,
                LensName: lensName,
                ComboPrice: combo.ComboPrice,
                OriginalTotalPrice: combo.OriginalTotalPrice,
                Savings: combo.SavingsAmount,
                IsActive: combo.IsActive,
                CreatedAt: combo.CreatedAt));
        }

        return result;
    }
}
