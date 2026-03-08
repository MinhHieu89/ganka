using Treatment.Domain.Entities;
using Treatment.Domain.Enums;

namespace Treatment.Application.Interfaces;

/// <summary>
/// Repository interface for TreatmentProtocol aggregate root persistence operations.
/// Provides protocol template catalog management with type-based filtering.
/// </summary>
public interface ITreatmentProtocolRepository
{
    /// <summary>
    /// Gets a treatment protocol by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    Task<TreatmentProtocol?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all treatment protocols, optionally including inactive (soft-deactivated) entries.
    /// Ordered by Name.
    /// </summary>
    Task<List<TreatmentProtocol>> GetAllAsync(bool includeInactive, CancellationToken ct);

    /// <summary>
    /// Gets all active treatment protocols filtered by treatment type.
    /// Ordered by Name.
    /// </summary>
    Task<List<TreatmentProtocol>> GetByTypeAsync(TreatmentType type, CancellationToken ct);

    /// <summary>
    /// Adds a new treatment protocol to the EF Core change tracker.
    /// Call IUnitOfWork.SaveChangesAsync to persist.
    /// </summary>
    void Add(TreatmentProtocol protocol);
}
