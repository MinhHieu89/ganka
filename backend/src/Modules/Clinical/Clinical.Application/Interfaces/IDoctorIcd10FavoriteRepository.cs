using Clinical.Domain.Entities;

namespace Clinical.Application.Interfaces;

/// <summary>
/// Repository interface for per-doctor ICD-10 favorites.
/// </summary>
public interface IDoctorIcd10FavoriteRepository
{
    /// <summary>
    /// Gets all ICD-10 codes favorited by a doctor.
    /// </summary>
    Task<List<string>> GetByDoctorIdAsync(Guid doctorId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a specific ICD-10 code is favorited by a doctor.
    /// </summary>
    Task<bool> ExistsAsync(Guid doctorId, string icd10Code, CancellationToken ct = default);

    /// <summary>
    /// Adds a new doctor ICD-10 favorite.
    /// </summary>
    Task AddAsync(DoctorIcd10Favorite favorite, CancellationToken ct = default);

    /// <summary>
    /// Removes a doctor's ICD-10 favorite by doctor ID and code.
    /// </summary>
    Task RemoveAsync(Guid doctorId, string icd10Code, CancellationToken ct = default);
}
