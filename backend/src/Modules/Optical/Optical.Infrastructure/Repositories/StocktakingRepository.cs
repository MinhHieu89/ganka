using Microsoft.EntityFrameworkCore;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Optical.Domain.Enums;

namespace Optical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IStocktakingRepository"/>.
/// Provides data access for StocktakingSession aggregate roots and their item entries.
/// All GetByIdAsync calls eagerly load Items for full aggregate mutation support.
/// GetCurrentSessionAsync returns the active InProgress session for the barcode scanning workflow.
/// </summary>
public sealed class StocktakingRepository(OpticalDbContext context) : IStocktakingRepository
{
    /// <summary>
    /// Gets a stocktaking session by ID with all item entries eagerly loaded.
    /// Returns null if not found.
    /// </summary>
    public async Task<StocktakingSession?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.StocktakingSessions
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Gets the currently active InProgress stocktaking session.
    /// Returns null when there is no active session.
    /// Eagerly loads Items for barcode scan recording.
    /// Only one InProgress session is expected at a time; returns the most recently created if multiple exist.
    /// </summary>
    public async Task<StocktakingSession?> GetCurrentSessionAsync(CancellationToken ct)
    {
        return await context.StocktakingSessions
            .Include(x => x.Items)
            .Where(x => x.Status == StocktakingStatus.InProgress)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Gets a paginated list of all stocktaking sessions (all statuses) ordered by CreatedAt descending.
    /// Eagerly loads Items for discrepancy summary display.
    /// </summary>
    public async Task<List<StocktakingSession>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        return await context.StocktakingSessions
            .Include(x => x.Items)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets the total count of all stocktaking sessions.
    /// Used for pagination metadata.
    /// </summary>
    public async Task<int> GetTotalCountAsync(CancellationToken ct)
    {
        return await context.StocktakingSessions.CountAsync(ct);
    }

    /// <summary>
    /// Adds a new stocktaking session to the EF Core change tracker.
    /// </summary>
    public void Add(StocktakingSession session)
    {
        context.StocktakingSessions.Add(session);
    }
}
