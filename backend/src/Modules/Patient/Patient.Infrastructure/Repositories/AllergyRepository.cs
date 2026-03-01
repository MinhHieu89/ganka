using Microsoft.EntityFrameworkCore;
using Patient.Application.Interfaces;
using Patient.Domain.Entities;

namespace Patient.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAllergyRepository"/>.
/// Direct allergy entity access for add/remove operations without loading the Patient aggregate.
/// </summary>
public sealed class AllergyRepository : IAllergyRepository
{
    private readonly PatientDbContext _dbContext;

    public AllergyRepository(PatientDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Allergy allergy)
    {
        _dbContext.Allergies.Add(allergy);
    }

    public void Remove(Allergy allergy)
    {
        _dbContext.Allergies.Remove(allergy);
    }

    public async Task<Allergy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Allergies.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
