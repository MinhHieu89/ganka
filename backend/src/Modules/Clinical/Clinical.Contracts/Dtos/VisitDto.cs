namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for visit summary data used in lists and search results.
/// </summary>
public record VisitDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    int CurrentStage,
    int Status,
    DateTime VisitDate,
    string? ExaminationNotes);
