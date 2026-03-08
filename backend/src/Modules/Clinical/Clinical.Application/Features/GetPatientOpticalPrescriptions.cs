using Clinical.Application.Interfaces;
using Optical.Contracts.Queries;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for the cross-module GetPatientOpticalPrescriptionsQuery.
/// Called by Optical module via IMessageBus to retrieve a patient's optical prescription history.
/// Returns prescriptions ordered by visit date descending (most recent first).
/// Satisfies OPT-08: cross-module prescription history integration.
/// </summary>
public static class GetPatientOpticalPrescriptionsHandler
{
    public static async Task<List<OpticalPrescriptionHistoryDto>> Handle(
        GetPatientOpticalPrescriptionsQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        return await visitRepository.GetOpticalPrescriptionsByPatientIdAsync(query.PatientId, ct);
    }
}
