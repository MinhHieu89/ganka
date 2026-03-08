using Optical.Domain.Entities;

namespace Optical.Application.Interfaces;

/// <summary>
/// Repository interface for StocktakingSession persistence operations.
/// Supports session lifecycle management, barcode scan recording, and discrepancy reporting.
/// </summary>
public interface IStocktakingRepository
{
    /// <summary>
    /// Gets a stocktaking session by ID with all its item entries included.
    /// Returns the domain entity for mutation (RecordItem, Complete, Cancel).
    /// </summary>
    Task<StocktakingSession?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets the currently active InProgress stocktaking session, if one exists.
    /// Returns null when there is no active session (only one InProgress session is expected at a time).
    /// </summary>
    Task<StocktakingSession?> GetCurrentSessionAsync(CancellationToken ct);

    /// <summary>
    /// Gets a paginated list of stocktaking sessions (all statuses) for the history view.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<List<StocktakingSession>> GetAllAsync(int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Gets the total count of stocktaking sessions.
    /// Used for pagination metadata in list queries.
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken ct);

    /// <summary>
    /// Adds a new stocktaking session to the EF Core change tracker.
    /// The caller must invoke IUnitOfWork.SaveChangesAsync to persist.
    /// </summary>
    void Add(StocktakingSession session);
}
