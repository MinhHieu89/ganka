using Clinical.Domain.Entities;

namespace Clinical.Application.Interfaces;

/// <summary>
/// Repository interface for OSDI submission CRUD operations.
/// Supports both staff-recorded and patient self-fill via public token.
/// </summary>
public interface IOsdiSubmissionRepository
{
    /// <summary>
    /// Adds a new OSDI submission to the repository.
    /// </summary>
    Task AddAsync(OsdiSubmission submission, CancellationToken ct = default);

    /// <summary>
    /// Gets an OSDI submission by its public token.
    /// Used for patient self-fill flow where the token is in the URL.
    /// </summary>
    Task<OsdiSubmission?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Gets the latest OSDI submission for a visit (most recent by CreatedAt).
    /// </summary>
    Task<OsdiSubmission?> GetByVisitIdAsync(Guid visitId, CancellationToken ct = default);

    /// <summary>
    /// Gets OSDI submissions for multiple visits (batch loading).
    /// Used for displaying OSDI history across visits.
    /// </summary>
    Task<List<OsdiSubmission>> GetByVisitIdsAsync(IEnumerable<Guid> visitIds, CancellationToken ct = default);
}
