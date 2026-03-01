namespace Scheduling.Contracts.Dtos;

/// <summary>
/// Command to book a new appointment (staff-initiated).
/// EndTime is computed from AppointmentType.DefaultDurationMinutes.
/// </summary>
public record BookAppointmentCommand(
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    DateTime StartTime,
    Guid AppointmentTypeId,
    string? Notes);

/// <summary>
/// Command to cancel an appointment with a mandatory reason.
/// </summary>
public record CancelAppointmentCommand(
    Guid AppointmentId,
    int CancellationReason,
    string? CancellationNote);

/// <summary>
/// Command to reschedule an appointment to a new time.
/// EndTime is recomputed from the appointment type's default duration.
/// </summary>
public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTime NewStartTime);

/// <summary>
/// Command to submit a public self-booking request (no auth required).
/// </summary>
public record SubmitSelfBookingCommand(
    string PatientName,
    string Phone,
    string? Email,
    Guid? PreferredDoctorId,
    DateTime PreferredDate,
    string? PreferredTimeSlot,
    Guid AppointmentTypeId,
    string? Notes);

/// <summary>
/// Command to approve a self-booking request and create an appointment.
/// Staff provides the specific doctor and start time.
/// </summary>
public record ApproveSelfBookingCommand(
    Guid SelfBookingRequestId,
    Guid DoctorId,
    string DoctorName,
    string PatientName,
    DateTime StartTime);

/// <summary>
/// Command to reject a self-booking request with a reason.
/// </summary>
public record RejectSelfBookingCommand(
    Guid SelfBookingRequestId,
    string Reason);
