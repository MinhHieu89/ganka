using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IDoctorIcd10FavoriteRepository"/>.
/// Pure data access -- no business logic, no SaveChanges.
/// </summary>
public sealed class DoctorIcd10FavoriteRepository : IDoctorIcd10FavoriteRepository
{
    private readonly ClinicalDbContext _dbContext;

    public DoctorIcd10FavoriteRepository(ClinicalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<string>> GetByDoctorIdAsync(Guid doctorId, CancellationToken ct = default)
    {
        return await _dbContext.DoctorIcd10Favorites
            .AsNoTracking()
            .Where(f => f.DoctorId == doctorId)
            .Select(f => f.Icd10Code)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid doctorId, string icd10Code, CancellationToken ct = default)
    {
        return await _dbContext.DoctorIcd10Favorites
            .AnyAsync(f => f.DoctorId == doctorId && f.Icd10Code == icd10Code, ct);
    }

    public async Task AddAsync(DoctorIcd10Favorite favorite, CancellationToken ct = default)
    {
        await _dbContext.DoctorIcd10Favorites.AddAsync(favorite, ct);
    }

    public async Task RemoveAsync(Guid doctorId, string icd10Code, CancellationToken ct = default)
    {
        var favorite = await _dbContext.DoctorIcd10Favorites
            .FirstOrDefaultAsync(f => f.DoctorId == doctorId && f.Icd10Code == icd10Code, ct);

        if (favorite is not null)
        {
            _dbContext.DoctorIcd10Favorites.Remove(favorite);
        }
    }
}
