using Clinical.Application.Interfaces;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clinical.Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="ITechnicianOrderQueryService"/>.
/// Uses ClinicalDbContext directly for read-side CQRS queries.
/// </summary>
public sealed class TechnicianOrderQueryService : ITechnicianOrderQueryService
{
    private readonly ClinicalDbContext _dbContext;

    public TechnicianOrderQueryService(ClinicalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static (DateTime todayStartUtc, DateTime todayEndUtc) GetTodayRange()
    {
        var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var nowVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz);
        var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(nowVietnam.Date, vietnamTz);
        var todayEndUtc = todayStartUtc.AddDays(1);
        return (todayStartUtc, todayEndUtc);
    }

    public async Task<List<TechnicianOrderWithVisitData>> GetTodayOrdersWithVisitsAsync(
        string? searchTerm, CancellationToken ct = default)
    {
        var (todayStart, todayEnd) = GetTodayRange();

        var query = _dbContext.TechnicianOrders
            .AsNoTracking()
            .Where(o => o.OrderedAt >= todayStart && o.OrderedAt < todayEnd)
            .Join(
                _dbContext.Visits.AsNoTracking().Where(v => !v.IsDeleted),
                o => o.VisitId,
                v => v.Id,
                (o, v) => new { Order = o, Visit = v });

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.Visit.PatientName.Contains(searchTerm));
        }

        return await query
            .Select(x => new TechnicianOrderWithVisitData(
                x.Order.Id,
                x.Visit.Id,
                x.Visit.PatientId,
                x.Visit.PatientName,
                x.Visit.VisitDate,
                x.Visit.Reason,
                x.Order.OrderType,
                x.Order.TechnicianId,
                x.Order.TechnicianName,
                x.Order.CompletedAt,
                x.Order.IsRedFlag,
                x.Order.RedFlagReason,
                x.Order.OrderedAt))
            .ToListAsync(ct);
    }

    public async Task<List<TechnicianOrderSummary>> GetTodayOrderSummariesAsync(CancellationToken ct = default)
    {
        var (todayStart, todayEnd) = GetTodayRange();

        return await _dbContext.TechnicianOrders
            .AsNoTracking()
            .Where(o => o.OrderedAt >= todayStart && o.OrderedAt < todayEnd)
            .Select(o => new TechnicianOrderSummary(o.TechnicianId, o.CompletedAt, o.IsRedFlag))
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, int>> GetVisitCountsByPatientIdsAsync(
        List<Guid> patientIds, CancellationToken ct = default)
    {
        return await _dbContext.Visits
            .AsNoTracking()
            .Where(v => !v.IsDeleted && patientIds.Contains(v.PatientId))
            .GroupBy(v => v.PatientId)
            .Select(g => new { PatientId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PatientId, x => x.Count, ct);
    }
}
