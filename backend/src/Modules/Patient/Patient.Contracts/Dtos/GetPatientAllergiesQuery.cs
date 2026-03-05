namespace Patient.Contracts.Dtos;

/// <summary>
/// Cross-module query to retrieve patient allergies.
/// Used by Clinical module for drug-allergy cross-checking.
/// </summary>
public sealed record GetPatientAllergiesQuery(Guid PatientId);
