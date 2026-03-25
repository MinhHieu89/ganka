namespace Clinical.Domain.Enums;

/// <summary>
/// Represents the stages of a patient's journey through the clinic.
/// Used for Kanban board positioning and workflow tracking.
/// Enum integer values do NOT need to match flow order -- AdvanceStage routing handles sequence.
/// Flow order: Reception -> RefractionVA -> DoctorExam -> [Imaging -> DoctorReviewsResults] -> Prescription -> [OpticalCenter] -> Cashier -> Pharmacy/OpticalLab -> [ReturnGlasses] -> Done
/// NO CashierGlasses -- single combined payment at Cashier.
/// </summary>
public enum WorkflowStage
{
    Reception = 0,
    RefractionVA = 1,
    DoctorExam = 2,
    Imaging = 3,
    DoctorReviewsResults = 4,
    Prescription = 5,
    Cashier = 6,
    Pharmacy = 7,
    OpticalCenter = 8,
    OpticalLab = 9,
    ReturnGlasses = 10,
    Done = 99
}
