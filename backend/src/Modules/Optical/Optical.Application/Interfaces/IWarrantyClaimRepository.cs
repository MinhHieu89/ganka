using Optical.Domain.Entities;

namespace Optical.Application.Interfaces;

/// <summary>
/// Repository interface for WarrantyClaim persistence operations.
/// Supports warranty claim creation, manager approval workflow, and paginated list queries.
/// </summary>
public interface IWarrantyClaimRepository
{
    /// <summary>
    /// Gets a warranty claim by ID (returns domain entity for mutation such as Approve/Reject).
    /// </summary>
    Task<WarrantyClaim?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all warranty claims for a specific glasses order.
    /// Used to display warranty claim history for an order (a single order can have multiple claims).
    /// </summary>
    Task<List<WarrantyClaim>> GetByOrderIdAsync(Guid glassesOrderId, CancellationToken ct);

    /// <summary>
    /// Gets a paginated list of warranty claims with optional approval status filter.
    /// </summary>
    /// <param name="approvalStatusFilter">Optional approval status to filter by (null returns all statuses).</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<List<WarrantyClaim>> GetAllAsync(int? approvalStatusFilter, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Gets the total count of warranty claims matching the optional approval status filter.
    /// Used for pagination metadata in list queries.
    /// </summary>
    Task<int> GetTotalCountAsync(int? approvalStatusFilter, CancellationToken ct);

    /// <summary>
    /// Adds a new warranty claim to the EF Core change tracker.
    /// The caller must invoke IUnitOfWork.SaveChangesAsync to persist.
    /// </summary>
    void Add(WarrantyClaim claim);
}
