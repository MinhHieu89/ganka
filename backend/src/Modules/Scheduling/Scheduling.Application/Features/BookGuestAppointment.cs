using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Command to book a guest appointment (no patient record, per D-11).
/// Full handler implementation provided by plan 14-02.
/// </summary>
public record BookGuestAppointmentCommand(
    string GuestName,
    string GuestPhone,
    string? GuestReason,
    Guid? DoctorId,
    string? DoctorName,
    DateTime StartTime,
    AppointmentSource Source);
