using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Application.Interfaces;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for searching ICD-10 codes.
/// Queries IReferenceDataRepository for bilingual search (English + Vietnamese descriptions).
/// Doctor favorites are pinned to the top of results.
/// </summary>
public static class SearchIcd10CodesHandler
{
    public static async Task<List<Icd10SearchResultDto>> Handle(
        SearchIcd10CodesQuery query,
        IReferenceDataRepository referenceDataRepository,
        IDoctorIcd10FavoriteRepository favoriteRepository,
        CancellationToken ct)
    {
        var term = query.SearchTerm.Trim();

        // Search by code, English description, or Vietnamese description
        var codes = await referenceDataRepository.SearchAsync(term, 50, ct);

        // Get doctor's favorites if DoctorId provided
        var favoriteCodes = new HashSet<string>();
        if (query.DoctorId.HasValue)
        {
            var favorites = await favoriteRepository.GetByDoctorIdAsync(query.DoctorId.Value, ct);
            favoriteCodes = favorites.ToHashSet();
        }

        // Map to DTOs with favorite status
        var results = codes
            .Select(c => new Icd10SearchResultDto(
                c.Code,
                c.DescriptionEn,
                c.DescriptionVi,
                c.Category,
                c.RequiresLaterality,
                favoriteCodes.Contains(c.Code)))
            .OrderByDescending(r => r.IsFavorite)
            .ThenBy(r => r.Code)
            .ToList();

        return results;
    }
}
