using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for getting a doctor's ICD-10 favorites with full code details.
/// Queries the per-doctor favorites and enriches with data from ReferenceDbContext.
/// </summary>
public static class GetDoctorFavoritesHandler
{
    public static async Task<List<Icd10SearchResultDto>> Handle(
        GetDoctorFavoritesQuery query,
        ReferenceDbContext referenceDb,
        IDoctorIcd10FavoriteRepository favoriteRepository,
        CancellationToken ct)
    {
        var favoriteCodes = await favoriteRepository.GetByDoctorIdAsync(query.DoctorId, ct);
        if (favoriteCodes.Count == 0)
            return [];

        var codes = await referenceDb.Icd10Codes
            .Where(c => favoriteCodes.Contains(c.Code))
            .OrderBy(c => c.Code)
            .ToListAsync(ct);

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
