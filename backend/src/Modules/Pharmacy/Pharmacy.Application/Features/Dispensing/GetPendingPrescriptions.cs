using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;

namespace Pharmacy.Application.Features.Dispensing;

/// <summary>
/// Query to retrieve pending (not yet dispensed) prescriptions for the pharmacy dispensing queue.
/// </summary>
/// <param name="PatientId">Optional patient filter. When null, returns all pending prescriptions for the branch.</param>
public sealed record GetPendingPrescriptionsQuery(Guid? PatientId = null);

/// <summary>
/// Wolverine static handler for retrieving the pharmacy dispensing queue.
///
/// Returns all prescriptions that have not yet been dispensed, enriched with expiry information.
/// The IDispensingRepository implementation resolves pending prescriptions via IMessageBus cross-module
/// query to the Clinical module (GetPendingPrescriptionsQuery in Clinical.Contracts).
///
/// PHR-05: Pharmacist can view the queue of pending prescriptions.
/// PHR-07: Expiry information (IsExpired, DaysRemaining) surfaced for each prescription.
/// </summary>
public static class GetPendingPrescriptionsHandler
{
    public static async Task<Result<List<PendingPrescriptionDto>>> Handle(
        GetPendingPrescriptionsQuery query,
        IDispensingRepository dispensingRepository,
        CancellationToken ct)
    {
        var items = await dispensingRepository.GetPendingPrescriptionsAsync(query.PatientId, ct);
        return items;
    }
}
