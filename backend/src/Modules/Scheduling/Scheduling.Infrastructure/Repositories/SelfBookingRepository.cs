using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISelfBookingRepository"/>.
/// Pure data access -- no business logic, no SaveChanges.
/// </summary>
public sealed class SelfBookingRepository : ISelfBookingRepository
{
    private readonly SchedulingDbContext _dbContext;

    public SelfBookingRepository(SchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SelfBookingRequest>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _dbContext.SelfBookingRequests
            .AsNoTracking()
            .Where(r => r.Status == BookingStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<SelfBookingRequest?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken ct = default)
    {
        return await _dbContext.SelfBookingRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReferenceNumber == referenceNumber, ct);
    }

    public async Task<int> CountPendingByPhoneAsync(string phone, CancellationToken ct = default)
    {
        return await _dbContext.SelfBookingRequests
            .CountAsync(r => r.Phone == phone && r.Status == BookingStatus.Pending, ct);
    }

    public async Task<SelfBookingRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.SelfBookingRequests.FindAsync([id], ct);
    }

    public void Add(SelfBookingRequest request)
    {
        _dbContext.SelfBookingRequests.Add(request);
    }
}
