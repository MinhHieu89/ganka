using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clinical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IMedicalImageRepository"/>.
/// Pure data access -- no business logic, no SaveChanges.
/// </summary>
public sealed class MedicalImageRepository : IMedicalImageRepository
{
    private readonly ClinicalDbContext _dbContext;

    public MedicalImageRepository(ClinicalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MedicalImage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.MedicalImages.FindAsync([id], ct);
    }

    public async Task<List<MedicalImage>> GetByVisitIdAsync(Guid visitId, CancellationToken ct = default)
    {
        return await _dbContext.MedicalImages
            .AsNoTracking()
            .Where(m => m.VisitId == visitId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<MedicalImage>> GetByVisitIdAndTypeAsync(Guid visitId, ImageType type, CancellationToken ct = default)
    {
        return await _dbContext.MedicalImages
            .AsNoTracking()
            .Where(m => m.VisitId == visitId && m.Type == type)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(MedicalImage image, CancellationToken ct = default)
    {
        await _dbContext.MedicalImages.AddAsync(image, ct);
    }

    public void Delete(MedicalImage image)
    {
        _dbContext.MedicalImages.Remove(image);
    }
}
