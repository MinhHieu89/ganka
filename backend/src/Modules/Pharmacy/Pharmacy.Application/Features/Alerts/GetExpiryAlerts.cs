using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Application.Features.Alerts;

/// <summary>
/// Query to retrieve drug batches nearing expiry within a configurable threshold.
/// Per PHR-03: configurable thresholds of 30, 60, or 90 days.
/// Excludes already-expired batches and zero-quantity batches (filtered in repository).
/// </summary>
public sealed record GetExpiryAlertsQuery(int DaysThreshold = 90);

/// <summary>
/// Wolverine static handler for retrieving expiry alerts.
/// Delegates to IDrugBatchRepository.GetExpiryAlertsAsync with the configured threshold.
/// </summary>
public static class GetExpiryAlertsHandler
{
    public static async Task<List<ExpiryAlertDto>> Handle(
        GetExpiryAlertsQuery query,
        IDrugBatchRepository repository,
        CancellationToken ct)
    {
        return await repository.GetExpiryAlertsAsync(query.DaysThreshold, ct);
    }
}
