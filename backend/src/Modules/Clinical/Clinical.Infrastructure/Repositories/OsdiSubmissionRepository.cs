using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOsdiSubmissionRepository"/>.
/// Pure data access -- no business logic, no SaveChanges.
/// </summary>
public sealed class OsdiSubmissionRepository : IOsdiSubmissionRepository
{
    private readonly ClinicalDbContext _dbContext;

    public OsdiSubmissionRepository(ClinicalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OsdiSubmission submission, CancellationToken ct = default)
    {
        await _dbContext.OsdiSubmissions.AddAsync(submission, ct);
    }

    public async Task<OsdiSubmission?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _dbContext.OsdiSubmissions
            .SingleOrDefaultAsync(o => o.PublicToken == token, ct);
    }

    public async Task<OsdiSubmission?> GetByVisitIdAsync(Guid visitId, CancellationToken ct = default)
    {
        return await _dbContext.OsdiSubmissions
            .Where(o => o.VisitId == visitId)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<OsdiSubmission>> GetByVisitIdsAsync(IEnumerable<Guid> visitIds, CancellationToken ct = default)
    {
        return await _dbContext.OsdiSubmissions
            .AsNoTracking()
            .Where(o => visitIds.Contains(o.VisitId))
            .ToListAsync(ct);
    }
}
