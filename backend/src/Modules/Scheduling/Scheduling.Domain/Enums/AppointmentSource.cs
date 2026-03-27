namespace Scheduling.Domain.Enums;

/// <summary>
/// How the appointment was created.
/// </summary>
public enum AppointmentSource
{
    /// <summary>Staff-initiated booking (in-person or phone).</summary>
    Staff = 0,

    /// <summary>Phone booking (guest, no patient record yet).</summary>
    Phone = 1,

    /// <summary>Web/online self-booking.</summary>
    Web = 2
}
