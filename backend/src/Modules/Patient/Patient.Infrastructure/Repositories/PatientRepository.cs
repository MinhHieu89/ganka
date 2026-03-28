using Microsoft.EntityFrameworkCore;
using Patient.Application.Interfaces;
using Patient.Domain.Enums;

namespace Patient.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPatientRepository"/>.
/// Pure data access -- no business logic, no SaveChanges.
/// </summary>
public sealed class PatientRepository : IPatientRepository
{
    private readonly PatientDbContext _dbContext;

    public PatientRepository(PatientDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Entities.Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Patients
            .AsNoTracking()
            .Include(p => p.Allergies)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.Patient?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Patients
            .Include(p => p.Allergies)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Domain.Entities.Patient>> SearchAsync(string term, int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Patients
            .AsNoTracking()
            .Where(p => p.IsActive &&
                (p.FullName.Contains(term) ||
                 p.Phone.Contains(term) ||
                 p.PatientCode.Contains(term)))
            .OrderBy(p => p.FullName)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Domain.Entities.Patient> Patients, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Gender? gender = null,
        bool? hasAllergies = null,
        DateTime? from = null,
        DateTime? to = null,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Patients
            .AsNoTracking()
            .Include(p => p.Allergies)
            .AsQueryable();

        // Filter by active status: null = show all, true = active only, false = inactive only
        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.FullName.Contains(term) ||
                p.Phone.Contains(term) ||
                p.PatientCode.Contains(term));
        }

        if (gender.HasValue)
            query = query.Where(p => p.Gender == gender.Value);

        if (hasAllergies == true)
            query = query.Where(p => p.Allergies.Any());
        else if (hasAllergies == false)
            query = query.Where(p => !p.Allergies.Any());

        if (from.HasValue)
            query = query.Where(p => p.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.CreatedAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var patients = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (patients, totalCount);
    }

    public async Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Patients.AnyAsync(p => p.Phone == phone, cancellationToken);
    }

    public async Task<int> GetMaxSequenceNumberForYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var max = await _dbContext.Patients
            .Where(p => p.Year == year && p.SequenceNumber > 0)
            .MaxAsync(p => (int?)p.SequenceNumber, cancellationToken);

        return max ?? 0;
    }

    public async Task<List<Domain.Entities.Patient>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Patients
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Patient>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0) return [];
        return await _dbContext.Patients
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Patients.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Patients.CountAsync(p => p.IsActive, cancellationToken);
    }

    public void Add(Domain.Entities.Patient patient)
    {
        _dbContext.Patients.Add(patient);
    }
}
