using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve all active treatment packages across all patients.
/// Used for the treatments overview page.
/// </summary>
public sealed record GetActiveTreatmentsQuery();

/// <summary>
/// Wolverine static handler for retrieving all active treatment packages.
/// Returns packages with Active status across all patients.
/// </summary>
public static class GetActiveTreatmentsHandler
{
    public static async Task<Result<List<TreatmentPackageDto>>> Handle(
        GetActiveTreatmentsQuery query,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        CancellationToken ct)
    {
        var packages = await packageRepository.GetActivePackagesAsync(ct);

        var dtos = new List<TreatmentPackageDto>();
        foreach (var package in packages)
        {
            var protocol = await protocolRepository.GetByIdAsync(package.ProtocolTemplateId, ct);
            var protocolName = protocol?.Name ?? "Unknown Protocol";
            dtos.Add(CreateTreatmentPackageHandler.MapToDto(package, protocolName));
        }

        return dtos;
    }
}
