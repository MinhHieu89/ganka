using Patient.Contracts.Enums;

namespace Patient.Contracts.Dtos;

public sealed record AllergyDto(
    Guid Id,
    string Name,
    AllergySeverity Severity);
