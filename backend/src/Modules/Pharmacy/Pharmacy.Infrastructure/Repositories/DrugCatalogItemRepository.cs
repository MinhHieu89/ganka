using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IDrugCatalogItemRepository"/>.
/// Leverages Vietnamese_CI_AI collation on Name, NameVi, and GenericName columns
/// for accent-insensitive, case-insensitive drug search at the database level.
/// </summary>
public sealed class DrugCatalogItemRepository : IDrugCatalogItemRepository
{
    private readonly PharmacyDbContext _dbContext;

    public DrugCatalogItemRepository(PharmacyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DrugCatalogItemDto>> SearchAsync(string searchTerm, CancellationToken ct)
    {
        return await _dbContext.DrugCatalogItems
            .AsNoTracking()
            .Where(d => d.IsActive &&
                (d.Name.Contains(searchTerm) ||
                 d.NameVi.Contains(searchTerm) ||
                 d.GenericName.Contains(searchTerm)))
            .OrderBy(d => d.Name)
            .Take(20)
            .Select(d => new DrugCatalogItemDto(
                d.Id,
                d.Name,
                d.NameVi,
                d.GenericName,
                (int)d.Form,
                d.Strength,
                (int)d.Route,
                d.Unit,
                d.DefaultDosageTemplate,
                d.IsActive))
            .ToListAsync(ct);
    }

    public async Task<DrugCatalogItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbContext.DrugCatalogItems
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<List<DrugCatalogItem>> GetAllActiveAsync(CancellationToken ct)
    {
        return await _dbContext.DrugCatalogItems
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public void Add(DrugCatalogItem item)
    {
        _dbContext.DrugCatalogItems.Add(item);
    }

    public void Update(DrugCatalogItem item)
    {
        _dbContext.DrugCatalogItems.Update(item);
    }
}
