using Optical.Contracts.Queries;
using Shared.Domain;
using Wolverine;

namespace Optical.Application.Features.Prescriptions;

/// <summary>
/// Query to retrieve optical prescription history for a patient via cross-module query.
/// Delegates to Clinical module using GetPatientOpticalPrescriptionsQuery.
/// </summary>
public sealed record GetPatientPrescriptionHistoryQuery(Guid PatientId);

/// <summary>
/// Wolverine static handler for retrieving a patient's optical prescription history.
/// Sends a cross-module query to the Clinical module via IMessageBus.
/// Returns prescriptions sorted by VisitDate descending (most recent first).
/// </summary>
public static class GetPatientPrescriptionHistoryHandler
{
    public static async Task<List<OpticalPrescriptionHistoryDto>> Handle(
        GetPatientPrescriptionHistoryQuery query,
        IMessageBus bus,
        CancellationToken ct)
    {
        var history = await bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
            new GetPatientOpticalPrescriptionsQuery(query.PatientId), ct);

        if (history is null || history.Count == 0)
            return [];

        return history
            .OrderByDescending(p => p.VisitDate)
            .ToList();
    }
}
