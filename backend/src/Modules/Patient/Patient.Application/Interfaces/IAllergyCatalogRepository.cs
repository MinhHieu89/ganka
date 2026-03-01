using Patient.Domain.Entities;

namespace Patient.Application.Interfaces;

/// <summary>
/// Repository interface for allergy catalog reference data.
/// </summary>
public interface IAllergyCatalogRepository
{
    Task<List<AllergyCatalogItem>> SearchAsync(string term, int limit = 20, CancellationToken cancellationToken = default);
    Task<List<AllergyCatalogItem>> GetAllAsync(CancellationToken cancellationToken = default);
}
