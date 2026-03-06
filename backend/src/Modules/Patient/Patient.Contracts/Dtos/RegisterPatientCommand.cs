using Patient.Contracts.Enums;

namespace Patient.Contracts.Dtos;

public sealed record RegisterPatientCommand(
    string FullName,
    string Phone,
    DateTime? DateOfBirth,
    Gender? Gender,
    PatientType PatientType,
    string? Address,
    string? Cccd,
    List<AllergyInput>? Allergies);

public sealed record AllergyInput(string Name, AllergySeverity Severity);
