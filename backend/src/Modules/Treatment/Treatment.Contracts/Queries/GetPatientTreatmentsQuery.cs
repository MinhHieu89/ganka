namespace Treatment.Contracts.Queries;

/// <summary>
/// Cross-module query to retrieve treatment packages for a specific patient.
/// Used by the Patient module to display the Treatments tab in patient profiles.
/// Handled by Treatment.Application.
/// </summary>
public sealed record GetPatientTreatmentsQuery(Guid PatientId);
