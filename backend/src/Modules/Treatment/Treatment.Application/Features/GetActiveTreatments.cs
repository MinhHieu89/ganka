using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;

namespace Treatment.Application.Features;

/// <summary>
/// Query to get all active treatment packages across all patients.
/// Stub created for compilation -- full implementation in plan 09-11.
/// </summary>
public sealed record GetActiveTreatmentsQuery();

/// <summary>
/// Wolverine handler for <see cref="GetActiveTreatmentsQuery"/>.
/// Stub -- full implementation in plan 09-11.
/// </summary>
public static class GetActiveTreatmentsHandler
{
    public static async Task<Result<List<TreatmentPackageDto>>> Handle(
        GetActiveTreatmentsQuery query,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Stub -- full implementation in plan 09-11.");
    }
}
