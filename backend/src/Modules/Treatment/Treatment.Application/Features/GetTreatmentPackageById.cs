using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve a treatment package by its ID with all child entities.
/// Returns full package with sessions, protocol versions, and cancellation request.
/// </summary>
public sealed record GetTreatmentPackageByIdQuery(Guid PackageId);

/// <summary>
/// Wolverine static handler for retrieving a full treatment package by ID.
/// Loads package with all child entities (Sessions, Versions, CancellationRequest)
/// and maps to TreatmentPackageDto with complete details.
/// </summary>
public static class GetTreatmentPackageByIdHandler
{
    public static async Task<Result<TreatmentPackageDto>> Handle(
        GetTreatmentPackageByIdQuery query,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        CancellationToken ct)
    {
        var package = await packageRepository.GetByIdAsync(query.PackageId, ct);
        if (package is null)
            return Result.Failure<TreatmentPackageDto>(
                Error.NotFound("TreatmentPackage", query.PackageId));

        var protocol = await protocolRepository.GetByIdAsync(package.ProtocolTemplateId, ct);
        var protocolName = protocol?.Name ?? "Unknown Protocol";

        return CreateTreatmentPackageHandler.MapToDto(package, protocolName);
    }
}
