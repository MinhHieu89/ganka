using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Domain.Entities;

/// <summary>
/// Aggregate root for patient self-booking requests submitted via the public booking page.
/// Follows an approval workflow: Pending -> Approved/Rejected by staff.
/// </summary>
public class SelfBookingRequest : AggregateRoot
{
    public string PatientName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public Guid? PreferredDoctorId { get; private set; }
    public DateTime PreferredDate { get; private set; }
    public string? PreferredTimeSlot { get; private set; }
    public Guid AppointmentTypeId { get; private set; }
    public string? Notes { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public BookingStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid? CreatedAppointmentId { get; private set; }

    private SelfBookingRequest() { }

    /// <summary>
    /// Factory method for creating a new self-booking request.
    /// Auto-generates a reference number in format BK-{yyMMdd}-{random4digits}.
    /// </summary>
    public static SelfBookingRequest Create(
        string patientName,
        string phone,
        string? email,
        Guid? preferredDoctorId,
        DateTime preferredDate,
        string? preferredTimeSlot,
        Guid appointmentTypeId,
        string? notes,
        BranchId branchId)
    {
        var request = new SelfBookingRequest
        {
            PatientName = patientName,
            Phone = phone,
            Email = email,
            PreferredDoctorId = preferredDoctorId,
            PreferredDate = preferredDate,
            PreferredTimeSlot = preferredTimeSlot,
            AppointmentTypeId = appointmentTypeId,
            Notes = notes,
            ReferenceNumber = GenerateReferenceNumber(),
            Status = BookingStatus.Pending
        };

        request.SetBranchId(branchId);
        return request;
    }

    /// <summary>
    /// Approves this booking request and links it to the created appointment.
    /// </summary>
    public void Approve(Guid appointmentId)
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending booking requests can be approved.");

        Status = BookingStatus.Approved;
        CreatedAppointmentId = appointmentId;
        SetUpdatedAt();
    }

    /// <summary>
    /// Rejects this booking request with a reason.
    /// </summary>
    public void Reject(string reason)
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending booking requests can be rejected.");

        Status = BookingStatus.Rejected;
        RejectionReason = reason;
        SetUpdatedAt();
    }

    private static string GenerateReferenceNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyMMdd");
        var randomPart = Random.Shared.Next(1000, 9999);
        return $"BK-{datePart}-{randomPart}";
    }
}
