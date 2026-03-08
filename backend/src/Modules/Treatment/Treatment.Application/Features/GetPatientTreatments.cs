using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve all treatment packages for a specific patient.
/// Used by the treatments page and patient profile.
/// </summary>
public sealed record GetPatientTreatmentsQuery(Guid PatientId);

/// <summary>
/// Wolverine static handler for retrieving patient treatment packages.
/// Returns list with computed SessionsCompleted/SessionsRemaining (TRT-02).
/// Also handles the cross-module GetPatientTreatmentsQuery from Treatment.Contracts.
/// </summary>
public static class GetPatientTreatmentsHandler
{
    public static async Task<Result<List<TreatmentPackageDto>>> Handle(
        GetPatientTreatmentsQuery query,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        CancellationToken ct)
    {
        var packages = await packageRepository.GetByPatientIdAsync(query.PatientId, ct);

        var dtos = new List<TreatmentPackageDto>();
        foreach (var package in packages)
        {
            var protocol = await protocolRepository.GetByIdAsync(package.ProtocolTemplateId, ct);
            var protocolName = protocol?.Name ?? "Unknown Protocol";
            dtos.Add(CreateTreatmentPackageHandler.MapToDto(package, protocolName));
        }

        return dtos;
    }

    /// <summary>
    /// Handles the cross-module query from Treatment.Contracts.
    /// Used by Patient module to display treatments tab in patient profiles.
    /// </summary>
    public static async Task<List<TreatmentPackageDto>> HandleCrossModule(
        Treatment.Contracts.Queries.GetPatientTreatmentsQuery query,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        CancellationToken ct)
    {
        var packages = await packageRepository.GetByPatientIdAsync(query.PatientId, ct);

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
