using Scheduling.Domain.Enums;
using Scheduling.Domain.Events;
using Shared.Domain;

namespace Scheduling.Domain.Entities;

/// <summary>
/// Appointment aggregate root. Represents a booked appointment between a patient and doctor.
/// Implements double-booking prevention via OverlapsWith method and IAuditable for audit tracking.
/// Uses denormalized PatientName/DoctorName to avoid cross-module joins.
/// </summary>
public class Appointment : AggregateRoot, IAuditable
{
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public Guid DoctorId { get; private set; }
    public string DoctorName { get; private set; } = string.Empty;
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public Guid AppointmentTypeId { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public CancellationReason? CancellationReason { get; private set; }
    public string? CancellationNote { get; private set; }
    public string? Notes { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private Appointment() { }

    /// <summary>
    /// Factory method for creating a new appointment.
    /// Status defaults to Confirmed for staff-created bookings.
    /// </summary>
    public static Appointment Create(
        Guid patientId,
        string patientName,
        Guid doctorId,
        string doctorName,
        DateTime startTime,
        DateTime endTime,
        Guid appointmentTypeId,
        BranchId branchId,
        string? notes = null)
    {
        var appointment = new Appointment
        {
            PatientId = patientId,
            PatientName = patientName,
            DoctorId = doctorId,
            DoctorName = doctorName,
            StartTime = startTime,
            EndTime = endTime,
            AppointmentTypeId = appointmentTypeId,
            Status = AppointmentStatus.Confirmed,
            Notes = notes
        };

        appointment.SetBranchId(branchId);
        appointment.AddDomainEvent(new AppointmentBookedEvent
        {
            AppointmentId = appointment.Id,
            PatientId = patientId,
            DoctorId = doctorId,
            StartTime = startTime
        });

        return appointment;
    }

    /// <summary>
    /// Checks if this appointment overlaps with a given time range.
    /// Used for double-booking prevention at the application level.
    /// </summary>
    public bool OverlapsWith(DateTime start, DateTime end)
    {
        return StartTime < end && EndTime > start;
    }

    /// <summary>
    /// Cancels this appointment with a mandatory reason.
    /// </summary>
    public void Cancel(CancellationReason reason, string? note)
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled.");

        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed appointment.");

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        CancellationNote = note;
        SetUpdatedAt();

        AddDomainEvent(new AppointmentCancelledEvent
        {
            AppointmentId = Id,
            Reason = reason
        });
    }

    /// <summary>
    /// Reschedules this appointment to a new time range.
    /// </summary>
    public void Reschedule(DateTime newStart, DateTime newEnd)
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule a cancelled appointment.");

        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot reschedule a completed appointment.");

        var oldStart = StartTime;
        StartTime = newStart;
        EndTime = newEnd;
        SetUpdatedAt();

        AddDomainEvent(new AppointmentRescheduledEvent
        {
            AppointmentId = Id,
            OldStart = oldStart,
            NewStart = newStart
        });
    }

    /// <summary>
    /// Marks this appointment as completed.
    /// </summary>
    public void Complete()
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled appointment.");

        Status = AppointmentStatus.Completed;
        SetUpdatedAt();
    }

    /// <summary>
    /// Confirms a pending appointment.
    /// </summary>
    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidOperationException("Only pending appointments can be confirmed.");

        Status = AppointmentStatus.Confirmed;
        SetUpdatedAt();
    }
}
