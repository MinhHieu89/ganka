namespace Scheduling.Domain.Enums;

/// <summary>
/// Represents the status of a self-booking request from a patient.
/// </summary>
public enum BookingStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
