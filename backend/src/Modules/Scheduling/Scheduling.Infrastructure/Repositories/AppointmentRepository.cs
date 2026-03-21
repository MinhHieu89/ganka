using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAppointmentRepository"/>.
/// Pure data access -- no business logic, no SaveChanges.
/// </summary>
public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly SchedulingDbContext _dbContext;

    public AppointmentRepository(SchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasOverlappingAsync(
        Guid doctorId, DateTime startTime, DateTime endTime,
        Guid? excludeAppointmentId = null, CancellationToken ct = default)
    {
        var query = _dbContext.Appointments
            .Where(a => a.DoctorId == doctorId
                && a.Status != AppointmentStatus.Cancelled
                && a.StartTime < endTime
                && a.EndTime > startTime);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<List<Appointment>> GetByDoctorAsync(
        Guid doctorId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId
                && a.StartTime >= dateFrom
                && a.StartTime < dateTo)
            .OrderBy(a => a.StartTime)
            .ToListAsync(ct);
    }

    public async Task<List<Appointment>> GetByPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync(ct);
    }

    public async Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken ct = default)
    {
        return await _dbContext.Appointments.FindAsync([appointmentId], ct);
    }

    public async Task<AppointmentType?> GetAppointmentTypeAsync(Guid appointmentTypeId, CancellationToken ct = default)
    {
        return await _dbContext.AppointmentTypes.FindAsync([appointmentTypeId], ct);
    }

    public async Task<List<AppointmentType>> GetAllAppointmentTypesAsync(CancellationToken ct = default)
    {
        return await _dbContext.AppointmentTypes
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<int> GetTodayCountAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return await _dbContext.Appointments
            .CountAsync(a => a.StartTime >= today && a.StartTime < tomorrow && a.Status != AppointmentStatus.Cancelled, ct);
    }

    public void Add(Appointment appointment)
    {
        _dbContext.Appointments.Add(appointment);
    }
}
