using Microsoft.EntityFrameworkCore;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Optical.Domain.Enums;

namespace Optical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IGlassesOrderRepository"/>.
/// Provides data access for the GlassesOrder aggregate root including its Items child collection.
/// All GetByIdAsync calls eagerly load Items to support full aggregate mutation.
/// GetOverdueOrdersAsync compares EstimatedDeliveryDate with UTC now for alert dashboard.
/// </summary>
public sealed class GlassesOrderRepository(OpticalDbContext context) : IGlassesOrderRepository
{
    /// <summary>
    /// Gets a glasses order by ID with all line items eagerly loaded.
    /// Returns null if not found.
    /// </summary>
    public async Task<GlassesOrder?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.GlassesOrders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Gets a paginated list of glasses orders with optional status filter.
    /// Eagerly loads Items. Ordered by CreatedAt descending (newest first).
    /// </summary>
    public async Task<List<GlassesOrder>> GetAllAsync(int? statusFilter, int page, int pageSize, CancellationToken ct)
    {
        var query = context.GlassesOrders
            .Include(x => x.Items)
            .AsNoTracking()
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            var status = (GlassesOrderStatus)statusFilter.Value;
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets all glasses orders for a specific patient, ordered by creation date descending.
    /// Eagerly loads Items for displaying order details.
    /// </summary>
    public async Task<List<GlassesOrder>> GetByPatientIdAsync(Guid patientId, CancellationToken ct)
    {
        return await context.GlassesOrders
            .Include(x => x.Items)
            .AsNoTracking()
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets all glasses orders linked to a specific clinical visit.
    /// Eagerly loads Items.
    /// </summary>
    public async Task<List<GlassesOrder>> GetByVisitIdAsync(Guid visitId, CancellationToken ct)
    {
        return await context.GlassesOrders
            .Include(x => x.Items)
            .AsNoTracking()
            .Where(x => x.VisitId == visitId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets all overdue glasses orders — EstimatedDeliveryDate has passed and status is not Delivered.
    /// Eagerly loads Items. Used by the overdue order alert dashboard.
    /// </summary>
    public async Task<List<GlassesOrder>> GetOverdueOrdersAsync(CancellationToken ct)
    {
        var utcNow = DateTime.UtcNow;

        return await context.GlassesOrders
            .Include(x => x.Items)
            .AsNoTracking()
            .Where(x =>
                x.EstimatedDeliveryDate != null &&
                x.EstimatedDeliveryDate < utcNow &&
                x.Status != GlassesOrderStatus.Delivered)
            .OrderBy(x => x.EstimatedDeliveryDate)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets the total count of glasses orders matching the optional status filter.
    /// Used for pagination metadata.
    /// </summary>
    public async Task<int> GetTotalCountAsync(int? statusFilter, CancellationToken ct)
    {
        var query = context.GlassesOrders.AsQueryable();

        if (statusFilter.HasValue)
        {
            var status = (GlassesOrderStatus)statusFilter.Value;
            query = query.Where(x => x.Status == status);
        }

        return await query.CountAsync(ct);
    }

    /// <summary>
    /// Adds a new glasses order to the EF Core change tracker.
    /// </summary>
    public void Add(GlassesOrder order)
    {
        context.GlassesOrders.Add(order);
    }
}
