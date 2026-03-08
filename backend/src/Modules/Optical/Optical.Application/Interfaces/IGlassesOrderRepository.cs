using Optical.Domain.Entities;

namespace Optical.Application.Interfaces;

/// <summary>
/// Repository interface for GlassesOrder persistence operations.
/// Supports order lifecycle management, patient order lookup, overdue order alerts, and paginated list queries.
/// </summary>
public interface IGlassesOrderRepository
{
    /// <summary>
    /// Gets a glasses order by ID with all its line items included.
    /// Returns the domain entity for mutation (status transitions, payment confirmation, adding items).
    /// </summary>
    Task<GlassesOrder?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets a paginated list of glasses orders with optional status filter.
    /// </summary>
    /// <param name="statusFilter">Optional status to filter by (null returns all statuses).</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<List<GlassesOrder>> GetAllAsync(int? statusFilter, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Gets all glasses orders for a specific patient.
    /// Used to display the patient's optical order history.
    /// </summary>
    Task<List<GlassesOrder>> GetByPatientIdAsync(Guid patientId, CancellationToken ct);

    /// <summary>
    /// Gets all glasses orders linked to a specific clinical visit.
    /// </summary>
    Task<List<GlassesOrder>> GetByVisitIdAsync(Guid visitId, CancellationToken ct);

    /// <summary>
    /// Gets all overdue glasses orders — orders where EstimatedDeliveryDate has passed
    /// and the status is not yet Delivered. Used by the overdue order alert dashboard.
    /// </summary>
    Task<List<GlassesOrder>> GetOverdueOrdersAsync(CancellationToken ct);

    /// <summary>
    /// Gets the total count of glasses orders matching the optional status filter.
    /// Used for pagination metadata in list queries.
    /// </summary>
    Task<int> GetTotalCountAsync(int? statusFilter, CancellationToken ct);

    /// <summary>
    /// Adds a new glasses order to the EF Core change tracker.
    /// The caller must invoke IUnitOfWork.SaveChangesAsync to persist.
    /// </summary>
    void Add(GlassesOrder order);
}
