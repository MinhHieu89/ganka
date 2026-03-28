using Patient.Contracts.Enums;

namespace Patient.Contracts.Dtos;

public sealed record PatientDto(
    Guid Id,
    string FullName,
    string Phone,
    string? PatientCode,
    PatientType PatientType,
    DateTime? DateOfBirth,
    Gender? Gender,
    string? Address,
    string? Cccd,
    string? Email,
    string? Occupation,
    string? PhotoUrl,
    bool IsActive,
    DateTime CreatedAt,
    List<AllergyDto> Allergies,
    string? OcularHistory,
    string? SystemicHistory,
    string? CurrentMedications,
    decimal? ScreenTimeHours,
    string? WorkEnvironment,
    string? ContactLensUsage,
    string? LifestyleNotes);
