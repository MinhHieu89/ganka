using Optical.Contracts.Queries;
using Shared.Domain;

namespace Optical.Application.Features.Prescriptions;

/// <summary>
/// Query to compare two prescriptions side by side with change indicators.
/// Handler implementation provided in plan 08-21.
/// </summary>
public sealed record GetPrescriptionComparisonQuery(Guid PatientId, Guid PrescriptionId1, Guid PrescriptionId2);

/// <summary>
/// Side-by-side comparison of two optical prescriptions with change indicators.
/// </summary>
public sealed record PrescriptionComparisonDto(
    OpticalPrescriptionHistoryDto Prescription1,
    OpticalPrescriptionHistoryDto Prescription2,
    PrescriptionChangesDto Changes);

/// <summary>
/// Change indicators between two prescriptions.
/// Positive values indicate increase, negative indicate decrease.
/// </summary>
public sealed record PrescriptionChangesDto(
    decimal? SphOdChange,
    decimal? CylOdChange,
    decimal? AxisOdChange,
    decimal? AddOdChange,
    decimal? SphOsChange,
    decimal? CylOsChange,
    decimal? AxisOsChange,
    decimal? AddOsChange,
    decimal? PdChange);
