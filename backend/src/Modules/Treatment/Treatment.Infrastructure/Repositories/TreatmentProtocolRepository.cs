using Microsoft.EntityFrameworkCore;
using Treatment.Application.Interfaces;
using Treatment.Domain.Entities;
using Treatment.Domain.Enums;

namespace Treatment.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ITreatmentProtocolRepository"/>.
/// Provides data access for TreatmentProtocol aggregate root (protocol templates).
/// Supports active-only filtering and treatment type filtering for protocol selection.
/// </summary>
public sealed class TreatmentProtocolRepository(TreatmentDbContext context) : ITreatmentProtocolRepository
{
    /// <summary>
    /// Gets a treatment protocol by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    public async Task<TreatmentProtocol?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.TreatmentProtocols
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Gets all treatment protocols, optionally including inactive (soft-deactivated) entries.
    /// Ordered by Name ascending.
    /// </summary>
    public async Task<List<TreatmentProtocol>> GetAllAsync(bool includeInactive, CancellationToken ct)
    {
        return await context.TreatmentProtocols
            .AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets all active treatment protocols filtered by treatment type.
    /// Ordered by Name ascending.
    /// </summary>
    public async Task<List<TreatmentProtocol>> GetByTypeAsync(TreatmentType type, CancellationToken ct)
    {
        return await context.TreatmentProtocols
            .AsNoTracking()
            .Where(x => x.TreatmentType == type && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Adds a new treatment protocol to the EF Core change tracker.
    /// </summary>
    public void Add(TreatmentProtocol protocol)
    {
        context.TreatmentProtocols.Add(protocol);
    }
}
