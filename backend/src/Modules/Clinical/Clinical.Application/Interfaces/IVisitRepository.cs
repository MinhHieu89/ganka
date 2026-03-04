using Clinical.Domain.Entities;

namespace Clinical.Application.Interfaces;

/// <summary>
/// Repository interface for the Visit aggregate root.
/// </summary>
public interface IVisitRepository
{
    /// <summary>
    /// Gets a visit by ID without related entities.
    /// </summary>
    Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a visit by ID with all related entities (Refractions, Diagnoses, Amendments).
    /// </summary>
    Task<Visit?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets active visits for the workflow dashboard.
    /// Returns visits that are Draft/Amended or were Signed within the last 24 hours.
    /// </summary>
    Task<List<Visit>> GetActiveVisitsAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a new visit to the repository.
    /// </summary>
    Task AddAsync(Visit visit, CancellationToken ct = default);

    /// <summary>
    /// Checks if a patient already has an active (non-signed) visit.
    /// </summary>
    Task<bool> HasActiveVisitForPatientAsync(Guid patientId, CancellationToken ct = default);
}
