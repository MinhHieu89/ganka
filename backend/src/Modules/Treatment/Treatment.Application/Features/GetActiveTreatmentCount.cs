using Treatment.Application.Interfaces;
using Treatment.Contracts.Queries;

namespace Treatment.Application.Features;

/// <summary>
/// Wolverine handler returning the count of active treatment packages.
/// Used by the Patient module dashboard via IMessageBus cross-module query.
/// </summary>
public static class GetActiveTreatmentCountHandler
{
    public static async Task<int> Handle(
        GetActiveTreatmentCountQuery query,
        ITreatmentPackageRepository treatmentPackageRepository,
        CancellationToken ct)
    {
        var activePackages = await treatmentPackageRepository.GetActivePackagesAsync(ct);
        return activePackages.Count;
    }
}
