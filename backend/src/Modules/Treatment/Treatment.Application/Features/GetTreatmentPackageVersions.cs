using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve all protocol version snapshots for a treatment package (TRT-07).
/// </summary>
public sealed record GetTreatmentPackageVersionsQuery(Guid PackageId);

/// <summary>
/// Wolverine static handler for <see cref="GetTreatmentPackageVersionsQuery"/>.
/// Loads the package and maps its version history to DTOs, ordered by version number descending.
/// </summary>
public static class GetTreatmentPackageVersionsHandler
{
    public static async Task<Result<List<ProtocolVersionDto>>> Handle(
        GetTreatmentPackageVersionsQuery query,
        ITreatmentPackageRepository packageRepository,
        CancellationToken ct)
    {
        var package = await packageRepository.GetByIdAsync(query.PackageId, ct);
        if (package is null)
            return Result<List<ProtocolVersionDto>>.Failure(
                Error.NotFound("TreatmentPackage", query.PackageId));

        var versions = package.Versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new ProtocolVersionDto(
                VersionNumber: v.VersionNumber,
                ChangeDescription: v.ChangeDescription,
                Reason: v.Reason,
                PreviousJson: v.PreviousJson,
                CurrentJson: v.CurrentJson,
                ChangedById: v.ChangedById,
                ChangedAt: v.CreatedAt))
            .ToList();

        return versions;
    }
}
