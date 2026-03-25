namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for active visit data displayed on the Kanban workflow dashboard.
/// Includes HasAllergies for the allergy warning icon and WaitMinutes for time tracking.
/// </summary>
public record ActiveVisitDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    string DoctorName,
    int CurrentStage,
    DateTime VisitDate,
    bool HasAllergies,
    int WaitMinutes,
    bool IsCompleted);
