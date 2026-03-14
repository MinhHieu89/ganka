using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Enums;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve treatment packages that are due for their next session.
/// Returns packages where the minimum interval has passed since the last session,
/// or packages with no sessions yet (immediately due).
/// </summary>
public sealed record GetDueSoonSessionsQuery();

/// <summary>
/// Wolverine static handler for the Due Soon dashboard query.
/// Delegates filtering to the repository (GetDueSoonAsync) and maps packages
/// to lightweight DTOs with computed NextDueDate.
/// </summary>
public static class GetDueSoonSessionsHandler
{
    public static async Task<List<TreatmentPackageDto>> Handle(
        GetDueSoonSessionsQuery query,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        CancellationToken ct)
    {
        var packages = await packageRepository.GetDueSoonAsync(ct);

        // Load protocol names for all packages
        var protocolIds = packages.Select(p => p.ProtocolTemplateId).Distinct().ToList();
        var protocolNames = new Dictionary<Guid, string>();
        foreach (var protocolId in protocolIds)
        {
            var protocol = await protocolRepository.GetByIdAsync(protocolId, ct);
            if (protocol is not null)
                protocolNames[protocolId] = protocol.Name;
        }

        return packages
            .Select(p => MapToDto(p, protocolNames.GetValueOrDefault(p.ProtocolTemplateId, "")))
            .ToList();
    }

    /// <summary>
    /// Maps a TreatmentPackage to a lightweight DTO with computed session dates.
    /// Calculates LastSessionDate from the most recent completed session
    /// and NextDueDate from LastSessionDate + MinIntervalDays.
    /// </summary>
    private static TreatmentPackageDto MapToDto(Treatment.Domain.Entities.TreatmentPackage package, string protocolTemplateName)
    {
        var completedSessions = package.Sessions
            .Where(s => s.Status == SessionStatus.Completed && s.CompletedAt.HasValue)
            .OrderByDescending(s => s.CompletedAt)
            .ToList();

        var lastSessionDate = completedSessions.Count > 0
            ? completedSessions[0].CompletedAt
            : null;

        var nextDueDate = lastSessionDate.HasValue
            ? lastSessionDate.Value.AddDays(package.MinIntervalDays)
            : package.CreatedAt; // If no sessions, due immediately (from creation date)

        var sessionDtos = package.Sessions
            .OrderBy(s => s.SessionNumber)
            .Select(s => RecordTreatmentSessionHandler.MapSessionToDto(s))
            .ToList();

        return new TreatmentPackageDto(
            Id: package.Id,
            ProtocolTemplateId: package.ProtocolTemplateId,
            ProtocolTemplateName: protocolTemplateName,
            PatientId: package.PatientId,
            PatientName: package.PatientName,
            TreatmentType: package.TreatmentType.ToString(),
            Status: package.Status.ToString(),
            TotalSessions: package.TotalSessions,
            SessionsCompleted: package.SessionsCompleted,
            SessionsRemaining: package.SessionsRemaining,
            PricingMode: package.PricingMode.ToString(),
            PackagePrice: package.PackagePrice,
            SessionPrice: package.SessionPrice,
            MinIntervalDays: package.MinIntervalDays,
            ParametersJson: package.ParametersJson,
            VisitId: package.VisitId,
            CreatedAt: package.CreatedAt,
            LastSessionDate: lastSessionDate,
            NextDueDate: nextDueDate,
            Sessions: sessionDtos,
            CancellationRequest: null); // Due Soon packages don't have cancellation requests
    }
}
