using Microsoft.EntityFrameworkCore;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Optical.Domain.Enums;

namespace Optical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IWarrantyClaimRepository"/>.
/// Provides data access for WarrantyClaim entities.
/// Supports approval status filtering for manager approval dashboard and paginated listing.
/// </summary>
public sealed class WarrantyClaimRepository(OpticalDbContext context) : IWarrantyClaimRepository
{
    /// <summary>
    /// Gets a warranty claim by ID.
    /// Returns the domain entity for mutation (Approve/Reject/AddDocumentUrl).
    /// Returns null if not found.
    /// </summary>
    public async Task<WarrantyClaim?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.WarrantyClaims
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Gets all warranty claims for a specific glasses order.
    /// Ordered by ClaimDate descending (newest claim first).
    /// A single order can have multiple claims over its warranty period.
    /// </summary>
    public async Task<List<WarrantyClaim>> GetByOrderIdAsync(Guid glassesOrderId, CancellationToken ct)
    {
        return await context.WarrantyClaims
            .AsNoTracking()
            .Where(x => x.GlassesOrderId == glassesOrderId)
            .OrderByDescending(x => x.ClaimDate)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets a paginated list of warranty claims with optional approval status filter.
    /// Ordered by CreatedAt descending (newest first).
    /// </summary>
    public async Task<List<WarrantyClaim>> GetAllAsync(int? approvalStatusFilter, int page, int pageSize, CancellationToken ct)
    {
        var query = context.WarrantyClaims
            .AsNoTracking()
            .AsQueryable();

        if (approvalStatusFilter.HasValue)
        {
            var status = (WarrantyApprovalStatus)approvalStatusFilter.Value;
            query = query.Where(x => x.ApprovalStatus == status);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets the total count of warranty claims matching the optional approval status filter.
    /// Used for pagination metadata.
    /// </summary>
    public async Task<int> GetTotalCountAsync(int? approvalStatusFilter, CancellationToken ct)
    {
        var query = context.WarrantyClaims.AsQueryable();

        if (approvalStatusFilter.HasValue)
        {
            var status = (WarrantyApprovalStatus)approvalStatusFilter.Value;
            query = query.Where(x => x.ApprovalStatus == status);
        }

        return await query.CountAsync(ct);
    }

    /// <summary>
    /// Adds a new warranty claim to the EF Core change tracker.
    /// </summary>
    public void Add(WarrantyClaim claim)
    {
        context.WarrantyClaims.Add(claim);
    }
}
