using Patient.Domain.Enums;

namespace Patient.Contracts.Dtos;

public sealed record PatientSearchResult(
    Guid Id,
    string FullName,
    string Phone,
    string? PatientCode,
    PatientType PatientType);
