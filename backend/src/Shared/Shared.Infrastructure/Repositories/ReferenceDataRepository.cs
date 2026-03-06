using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Domain;

namespace Shared.Infrastructure.Repositories;

/// <summary>
/// Implementation of IReferenceDataRepository using ReferenceDbContext.
/// Wraps existing ICD-10 queries for clean architecture compliance.
/// </summary>
public class ReferenceDataRepository : IReferenceDataRepository
{
    private readonly ReferenceDbContext _context;

    public ReferenceDataRepository(ReferenceDbContext context)
    {
        _context = context;
    }

    public async Task<List<Icd10Code>> SearchAsync(string term, int limit = 50, CancellationToken ct = default)
    {
        return await _context.Icd10Codes
            .Where(c =>
                c.Code.Contains(term) ||
                c.DescriptionEn.Contains(term) ||
                c.DescriptionVi.Contains(term))
            .OrderBy(c => c.Code)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<Icd10Code>> GetByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default)
    {
        return await _context.Icd10Codes
            .Where(c => codes.Contains(c.Code))
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }
}
