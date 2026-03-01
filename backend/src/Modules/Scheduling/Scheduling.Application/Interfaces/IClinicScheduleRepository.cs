using Scheduling.Domain.Entities;

namespace Scheduling.Application.Interfaces;

/// <summary>
/// Repository interface for clinic operating schedule.
/// </summary>
public interface IClinicScheduleRepository
{
    /// <summary>
    /// Returns the schedule for a specific day of the week.
    /// </summary>
    Task<ClinicSchedule?> GetForDayAsync(DayOfWeek day, CancellationToken ct = default);

    /// <summary>
    /// Returns the full week schedule (7 days).
    /// </summary>
    Task<List<ClinicSchedule>> GetAllAsync(CancellationToken ct = default);
}
