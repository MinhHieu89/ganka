using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Wolverine handler for getting available 30-min time slots for a date (D-10).
/// Uses ClinicSchedule for operating hours and counts booked appointments per slot.
/// </summary>
public static class GetAvailableSlotsHandler
{
    public static async Task<Result<List<AvailableSlotDto>>> Handle(
        GetAvailableSlotsQuery query,
        IClinicScheduleRepository clinicScheduleRepository,
        IAppointmentRepository appointmentRepository,
        CancellationToken ct)
    {
        // Convert to Vietnam local time for schedule lookup
        var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        var localDate = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(query.Date, DateTimeKind.Utc), vietnamTz);

        var schedule = await clinicScheduleRepository.GetForDayAsync(localDate.DayOfWeek, ct);
        if (schedule is null || !schedule.IsOpen || schedule.OpenTime is null || schedule.CloseTime is null)
            return Result<List<AvailableSlotDto>>.Success([]);

        var slots = new List<AvailableSlotDto>();
        const int slotDurationMinutes = 30; // D-10: hardcoded 30-min

        var currentSlotLocal = localDate.Date.Add(schedule.OpenTime.Value);
        var closeLocal = localDate.Date.Add(schedule.CloseTime.Value);

        // Get all appointments for the day (if doctor specified, filter by doctor)
        var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(localDate.Date, vietnamTz);
        var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(localDate.Date.AddDays(1), vietnamTz);

        List<Domain.Entities.Appointment> dayAppointments;
        if (query.DoctorId.HasValue)
        {
            dayAppointments = await appointmentRepository.GetByDoctorAsync(
                query.DoctorId.Value, dayStartUtc, dayEndUtc, ct);
        }
        else
        {
            dayAppointments = await appointmentRepository.GetTodayAppointmentsAsync(ct: ct);
            // Filter to the requested date
            dayAppointments = dayAppointments
                .Where(a => a.StartTime >= dayStartUtc && a.StartTime < dayEndUtc)
                .ToList();
        }

        while (currentSlotLocal.AddMinutes(slotDurationMinutes) <= closeLocal)
        {
            var slotStartUtc = TimeZoneInfo.ConvertTimeToUtc(currentSlotLocal, vietnamTz);
            var slotEndUtc = slotStartUtc.AddMinutes(slotDurationMinutes);

            var bookedCount = dayAppointments.Count(a =>
                a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                a.Status != Domain.Enums.AppointmentStatus.NoShow &&
                a.OverlapsWith(slotStartUtc, slotEndUtc));

            // If doctor specified, slot is available if no bookings. Otherwise just show count.
            var isAvailable = query.DoctorId.HasValue ? bookedCount == 0 : true;

            slots.Add(new AvailableSlotDto(slotStartUtc, slotEndUtc, isAvailable, bookedCount));
            currentSlotLocal = currentSlotLocal.AddMinutes(slotDurationMinutes);
        }

        return Result<List<AvailableSlotDto>>.Success(slots);
    }
}
