namespace Clinical.Domain.Enums;

/// <summary>
/// Represents the 8 stages of a patient's journey through the clinic.
/// Used for Kanban board positioning and workflow tracking.
/// </summary>
public enum WorkflowStage
{
    Reception = 0,
    RefractionVA = 1,
    DoctorExam = 2,
    Diagnostics = 3,
    DoctorReads = 4,
    Rx = 5,
    Cashier = 6,
    PharmacyOptical = 7
}
