namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for patient visit history display (D-13/D-15).
/// Shows visit summary with primary diagnosis text.
/// </summary>
public record PatientVisitHistoryDto(
    Guid VisitId,
    DateTime VisitDate,
    string DoctorName,
    int Status,
    string? PrimaryDiagnosisText,
    int CurrentStage);
