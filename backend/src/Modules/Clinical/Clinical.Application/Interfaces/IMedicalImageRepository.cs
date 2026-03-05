using Clinical.Domain.Entities;
using Clinical.Domain.Enums;

namespace Clinical.Application.Interfaces;

/// <summary>
/// Repository interface for medical image CRUD operations.
/// MedicalImage is independent from Visit aggregate -- append-only even after sign-off.
/// </summary>
public interface IMedicalImageRepository
{
    /// <summary>
    /// Gets a medical image by ID.
    /// </summary>
    Task<MedicalImage?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all medical images for a visit, ordered by CreatedAt descending.
    /// </summary>
    Task<List<MedicalImage>> GetByVisitIdAsync(Guid visitId, CancellationToken ct = default);

    /// <summary>
    /// Gets medical images for a visit filtered by image type.
    /// Used for same-type cross-visit comparison.
    /// </summary>
    Task<List<MedicalImage>> GetByVisitIdAndTypeAsync(Guid visitId, ImageType type, CancellationToken ct = default);

    /// <summary>
    /// Adds a new medical image record to the repository.
    /// </summary>
    Task AddAsync(MedicalImage image, CancellationToken ct = default);

    /// <summary>
    /// Marks a medical image for deletion.
    /// </summary>
    void Delete(MedicalImage image);
}
