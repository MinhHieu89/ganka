using Optical.Contracts.Dtos;
using Optical.Contracts.Queries;
using Shared.Domain;

namespace Optical.Application.Features.Prescriptions;

/// <summary>
/// Query to retrieve optical prescription history for a patient via cross-module query.
/// Delegates to Clinical module using GetPatientOpticalPrescriptionsQuery.
/// Handler implementation provided in plan 08-21.
/// </summary>
public sealed record GetPatientPrescriptionHistoryQuery(Guid PatientId);
