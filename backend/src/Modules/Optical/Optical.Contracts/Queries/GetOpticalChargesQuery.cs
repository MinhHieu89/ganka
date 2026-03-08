namespace Optical.Contracts.Queries;

/// <summary>
/// Cross-module query sent to Clinical module to retrieve a patient's optical prescription history.
/// Handled by Clinical.Application, returns a list of OpticalPrescriptionHistoryDto.
/// Used by Optical module to display historical prescription data when creating new glasses orders.
/// Satisfies OPT-08 cross-module prescription history integration.
/// </summary>
public sealed record GetPatientOpticalPrescriptionsQuery(Guid PatientId);

/// <summary>
/// DTO returned by Clinical module for a patient's optical prescription at a specific visit.
/// All refraction values in diopters. Pd (pupillary distance) in millimeters.
/// OD = right eye (oculus dexter), OS = left eye (oculus sinister).
/// </summary>
public sealed record OpticalPrescriptionHistoryDto(
    Guid Id,
    Guid VisitId,
    DateTime VisitDate,
    decimal? SphOd,
    decimal? CylOd,
    decimal? AxisOd,
    decimal? AddOd,
    decimal? SphOs,
    decimal? CylOs,
    decimal? AxisOs,
    decimal? AddOs,
    decimal? Pd,
    string? Notes);

/// <summary>
/// Cross-module query sent to Pharmacy module to retrieve suppliers tagged as optical type.
/// Handled by Pharmacy.Application, returns supplier list for frame/lens procurement.
/// Used by Optical module when selecting suppliers for glasses orders and frame stock.
/// </summary>
public sealed record GetOpticalSuppliersQuery();
