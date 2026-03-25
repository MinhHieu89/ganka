namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for full visit details including refractions, diagnoses, and amendments.
/// Used on the visit detail page (single scrollable medical chart).
/// </summary>
public record VisitDetailDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    int CurrentStage,
    int Status,
    DateTime VisitDate,
    string? ExaminationNotes,
    List<RefractionDto> Refractions,
    List<VisitDiagnosisDto> Diagnoses,
    List<VisitAmendmentDto> Amendments,
    List<DryEyeAssessmentDto> DryEyeAssessments,
    List<DrugPrescriptionDto> DrugPrescriptions,
    List<OpticalPrescriptionDto> OpticalPrescriptions,
    DateTime? SignedAt,
    Guid? SignedById,
    Guid? AppointmentId,
    bool ImagingRequested,
    bool RefractionSkipped);
