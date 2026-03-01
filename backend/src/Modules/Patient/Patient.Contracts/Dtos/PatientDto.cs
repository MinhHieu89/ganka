using Patient.Domain.Enums;

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
    string? PhotoUrl,
    bool IsActive,
    DateTime CreatedAt,
    List<AllergyDto> Allergies);
