using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Application.Interfaces;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for getting a doctor's ICD-10 favorites with full code details.
/// Queries the per-doctor favorites and enriches with data from IReferenceDataRepository.
/// </summary>
public static class GetDoctorFavoritesHandler
{
    public static async Task<List<Icd10SearchResultDto>> Handle(
        GetDoctorFavoritesQuery query,
        IReferenceDataRepository referenceDataRepository,
        IDoctorIcd10FavoriteRepository favoriteRepository,
        CancellationToken ct)
    {
        var favoriteCodes = await favoriteRepository.GetByDoctorIdAsync(query.DoctorId, ct);
        if (favoriteCodes.Count == 0)
            return [];

        var codes = await referenceDataRepository.GetByCodesAsync(favoriteCodes, ct);

        return codes.Select(c => new Icd10SearchResultDto(
            c.Code,
            c.DescriptionEn,
            c.DescriptionVi,
            c.Category,
            c.RequiresLaterality,
            true
        )).ToList();
    }
}
