using Patient.Contracts.Enums;

namespace Patient.Contracts.Dtos;

public sealed record UpdatePatientCommand(
    Guid PatientId,
    string FullName,
    string Phone,
    DateTime? DateOfBirth,
    Gender? Gender,
    string? Address,
    string? Cccd);
