using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IClinicScheduleRepository"/>.
/// Pure data access -- no business logic, no SaveChanges.
/// </summary>
public sealed class ClinicScheduleRepository : IClinicScheduleRepository
{
    private readonly SchedulingDbContext _dbContext;

    public ClinicScheduleRepository(SchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClinicSchedule?> GetForDayAsync(DayOfWeek day, CancellationToken ct = default)
    {
        return await _dbContext.ClinicSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.DayOfWeek == day, ct);
    }

    public async Task<List<ClinicSchedule>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.ClinicSchedules
            .AsNoTracking()
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync(ct);
    }
}
