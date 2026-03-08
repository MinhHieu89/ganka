using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve all sessions for a treatment package, ordered by session number.
/// </summary>
public sealed record GetTreatmentSessionsQuery(Guid PackageId);

/// <summary>
/// Wolverine static handler for retrieving treatment sessions by package ID.
/// Loads the package with eagerly loaded sessions and maps to DTOs ordered by SessionNumber.
/// </summary>
public static class GetTreatmentSessionsHandler
{
    public static async Task<Result<List<TreatmentSessionDto>>> Handle(
        GetTreatmentSessionsQuery query,
        ITreatmentPackageRepository packageRepository,
        CancellationToken ct)
    {
        var package = await packageRepository.GetByIdAsync(query.PackageId, ct);
        if (package is null)
            return Result<List<TreatmentSessionDto>>.Failure(
                Error.NotFound("TreatmentPackage", query.PackageId));

        var sessions = package.Sessions
            .OrderBy(s => s.SessionNumber)
            .Select(s => RecordTreatmentSessionHandler.MapSessionToDto(s))
            .ToList();

        return sessions;
    }
}
