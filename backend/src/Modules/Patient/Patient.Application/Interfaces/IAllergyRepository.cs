using Patient.Domain.Entities;

namespace Patient.Application.Interfaces;

/// <summary>
/// Repository interface for direct Allergy entity access.
/// Used for adding/removing allergies without loading the full Patient aggregate.
/// </summary>
public interface IAllergyRepository
{
    void Add(Allergy allergy);
    void Remove(Allergy allergy);
    Task<Allergy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
