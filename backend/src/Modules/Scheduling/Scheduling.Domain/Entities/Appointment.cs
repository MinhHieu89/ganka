using Scheduling.Domain.Enums;
using Scheduling.Domain.Events;
using Shared.Domain;

namespace Scheduling.Domain.Entities;

/// <summary>
/// Appointment aggregate root. Represents a booked appointment between a patient and doctor.
/// Supports both patient-linked and guest bookings (nullable PatientId).
/// Implements double-booking prevention via OverlapsWith method and IAuditable for audit tracking.
/// Uses denormalized PatientName/DoctorName to avoid cross-module joins.
/// </summary>
public class Appointment : AggregateRoot, IAuditable
{
    public Guid? PatientId { get; private set; }
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

    // Guest booking fields
    public string? GuestName { get; private set; }
    public string? GuestPhone { get; private set; }
    public string? GuestReason { get; private set; }

    // Check-in tracking
    public DateTime? CheckedInAt { get; private set; }

    // Source tracking
    public AppointmentSource Source { get; private set; }

    // No-show tracking
    public DateTime? NoShowAt { get; private set; }
    public Guid? NoShowBy { get; private set; }
    public string? NoShowNotes { get; private set; }

    // Cancellation audit
    public Guid? CancelledBy { get; private set; }

    private Appointment() { }

    /// <summary>
    /// Factory method for creating a new patient-linked appointment.
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
        string? notes = null,
        AppointmentSource source = AppointmentSource.Staff)
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
            Notes = notes,
            Source = source
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
    /// Factory method for creating a guest (non-patient-linked) appointment.
    /// PatientId is null; guest info stored in GuestName/GuestPhone/GuestReason.
    /// </summary>
    public static Appointment CreateGuest(
        string guestName,
        string guestPhone,
        string? guestReason,
        Guid doctorId,
        string doctorName,
        DateTime startTime,
        DateTime endTime,
        Guid appointmentTypeId,
        BranchId branchId,
        AppointmentSource source,
        string? notes = null)
    {
        var appointment = new Appointment
        {
            PatientId = null,
            PatientName = guestName,
            GuestName = guestName,
            GuestPhone = guestPhone,
            GuestReason = guestReason,
            DoctorId = doctorId,
            DoctorName = doctorName,
            StartTime = startTime,
            EndTime = endTime,
            AppointmentTypeId = appointmentTypeId,
            Status = AppointmentStatus.Confirmed,
            Notes = notes,
            Source = source
        };

        appointment.SetBranchId(branchId);
        appointment.AddDomainEvent(new AppointmentBookedEvent
        {
            AppointmentId = appointment.Id,
            PatientId = Guid.Empty,
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
    /// Records patient check-in. Only confirmed appointments can be checked in.
    /// </summary>
    public void CheckIn()
    {
        if (CheckedInAt is not null)
            throw new InvalidOperationException("Patient has already checked in.");

        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot check in to a cancelled appointment.");

        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed appointments can be checked in.");

        CheckedInAt = DateTime.UtcNow;
        SetUpdatedAt();

        AddDomainEvent(new AppointmentCheckedInEvent
        {
            AppointmentId = Id,
            PatientId = PatientId,
            CheckedInAt = CheckedInAt.Value
        });
    }

    /// <summary>
    /// Marks appointment as no-show with audit information.
    /// </summary>
    public void MarkNoShow(Guid userId, string? notes)
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot mark a cancelled appointment as no-show.");

        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot mark a completed appointment as no-show.");

        if (Status == AppointmentStatus.NoShow)
            throw new InvalidOperationException("Appointment is already marked as no-show.");

        Status = AppointmentStatus.NoShow;
        NoShowAt = DateTime.UtcNow;
        NoShowBy = userId;
        NoShowNotes = notes;
        SetUpdatedAt();

        AddDomainEvent(new AppointmentNoShowEvent
        {
            AppointmentId = Id,
            NoShowBy = userId
        });
    }

    /// <summary>
    /// Cancels this appointment with a mandatory reason.
    /// </summary>
    public void Cancel(CancellationReason reason, string? note, Guid? cancelledBy = null)
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled.");

        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed appointment.");

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        CancellationNote = note;
        CancelledBy = cancelledBy;
        SetUpdatedAt();

        AddDomainEvent(new AppointmentCancelledEvent
        {
            AppointmentId = Id,
            Reason = reason
        });
    }

    /// <summary>
    /// Links a newly registered patient to this guest appointment.
    /// </summary>
    public void LinkPatient(Guid patientId, string patientName)
    {
        PatientId = patientId;
        PatientName = patientName;
        SetUpdatedAt();
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
