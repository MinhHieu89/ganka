namespace Clinical.Domain.Enums;

/// <summary>
/// Reason for skipping a workflow stage (e.g., RefractionVA skip).
/// </summary>
public enum SkipReason
{
    FollowUpExisting = 0,
    PatientRefused = 1,
    UnrelatedExam = 2,
    Other = 3
}
