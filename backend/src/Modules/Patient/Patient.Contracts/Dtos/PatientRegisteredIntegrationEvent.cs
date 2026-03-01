namespace Patient.Contracts.Dtos;

/// <summary>
/// Integration event for cross-module consumption when a patient is registered.
/// </summary>
public sealed record PatientRegisteredIntegrationEvent(
    Guid Id,
    string PatientCode,
    string FullName,
    string Phone);
