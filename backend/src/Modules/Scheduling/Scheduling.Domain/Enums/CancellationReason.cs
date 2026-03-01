namespace Scheduling.Domain.Enums;

/// <summary>
/// Reasons for appointment cancellation. Required when cancelling.
/// </summary>
public enum CancellationReason
{
    PatientNoShow = 0,
    PatientRequest = 1,
    DoctorUnavailable = 2,
    Other = 3
}
