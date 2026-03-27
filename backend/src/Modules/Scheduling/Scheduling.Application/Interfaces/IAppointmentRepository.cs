using Scheduling.Domain.Entities;

namespace Scheduling.Application.Interfaces;

/// <summary>
/// Repository interface for the Appointment aggregate root.
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Checks if any non-cancelled appointment for the doctor overlaps with the given time range.
    /// Optionally excludes a specific appointment (for rescheduling).
    /// </summary>
    Task<bool> HasOverlappingAsync(Guid doctorId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null, CancellationToken ct = default);

    /// <summary>
    /// Returns appointments for a doctor within a date range (for calendar display).
    /// </summary>
    Task<List<Appointment>> GetByDoctorAsync(Guid doctorId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);

    /// <summary>
    /// Returns all appointments for a patient.
    /// </summary>
    Task<List<Appointment>> GetByPatientAsync(Guid patientId, CancellationToken ct = default);

    Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentType?> GetAppointmentTypeAsync(Guid appointmentTypeId, CancellationToken ct = default);
    Task<List<AppointmentType>> GetAllAppointmentTypesAsync(CancellationToken ct = default);
    /// <summary>
    /// Returns the count of appointments for the current day.
    /// </summary>
    Task<int> GetTodayCountAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns all appointments for today (for receptionist dashboard).
    /// </summary>
    Task<List<Appointment>> GetTodayAppointmentsAsync(CancellationToken ct = default);

    void Add(Appointment appointment);
}
