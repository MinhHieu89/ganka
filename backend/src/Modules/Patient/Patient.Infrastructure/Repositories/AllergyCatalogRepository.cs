using Microsoft.EntityFrameworkCore;
using Patient.Application.Interfaces;
using Patient.Domain.Entities;

namespace Patient.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAllergyCatalogRepository"/>.
/// Provides autocomplete and reference data access for allergy catalog items.
/// </summary>
public sealed class AllergyCatalogRepository : IAllergyCatalogRepository
{
    private readonly PatientDbContext _dbContext;

    public AllergyCatalogRepository(PatientDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AllergyCatalogItem>> SearchAsync(string term, int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AllergyCatalogItems
            .AsNoTracking()
            .Where(a => a.Name.Contains(term) || a.NameVi.Contains(term))
            .OrderBy(a => a.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AllergyCatalogItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AllergyCatalogItems
            .AsNoTracking()
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }
}
