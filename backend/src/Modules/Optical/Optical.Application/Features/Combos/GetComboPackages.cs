using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;

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
    public static async Task<List<ComboPackageDto>> Handle(
        GetComboPackagesQuery query,
        IComboPackageRepository repository,
        IFrameRepository frameRepository,
        ILensCatalogRepository lensRepository,
        CancellationToken ct)
    {
        var combos = await repository.GetAllAsync(query.IncludeInactive, ct);

        var result = new List<ComboPackageDto>(combos.Count);

        foreach (var combo in combos)
        {
            string? frameName = null;
            if (combo.FrameId.HasValue)
            {
                var frame = await frameRepository.GetByIdAsync(combo.FrameId.Value, ct);
                if (frame is not null)
                    frameName = $"{frame.Brand} {frame.Model}";
            }

            string? lensName = null;
            if (combo.LensCatalogItemId.HasValue)
            {
                var lens = await lensRepository.GetByIdAsync(combo.LensCatalogItemId.Value, ct);
                if (lens is not null)
                    lensName = $"{lens.Brand} {lens.Name}";
            }

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
