using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Entities;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve all treatment packages with PendingCancellation status.
/// Used to populate the manager approval queue.
/// </summary>
public sealed record GetPendingCancellationsQuery();

/// <summary>
/// Wolverine handler for <see cref="GetPendingCancellationsQuery"/>.
/// Returns all packages with PendingCancellation status, including cancellation request details.
/// </summary>
public static class GetPendingCancellationsHandler
{
    public static async Task<Result<List<TreatmentPackageDto>>> Handle(
        GetPendingCancellationsQuery query,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        CancellationToken cancellationToken)
    {
        var packages = await packageRepository.GetPendingCancellationsAsync(cancellationToken);

        // Load protocol names for all packages
        var protocolIds = packages.Select(p => p.ProtocolTemplateId).Distinct().ToList();
        var protocolNames = new Dictionary<Guid, string>();
        foreach (var protocolId in protocolIds)
        {
            var protocol = await protocolRepository.GetByIdAsync(protocolId, cancellationToken);
            if (protocol is not null)
                protocolNames[protocolId] = protocol.Name;
        }

        var dtos = packages.Select(p => MapToDto(p, protocolNames.GetValueOrDefault(p.ProtocolTemplateId, ""))).ToList();

        return Result<List<TreatmentPackageDto>>.Success(dtos);
    }

    private static TreatmentPackageDto MapToDto(TreatmentPackage package, string protocolTemplateName)
    {
        var sessions = package.Sessions.Select(s => new TreatmentSessionDto(
            Id: s.Id,
            SessionNumber: s.SessionNumber,
            Status: s.Status.ToString(),
            ParametersJson: s.ParametersJson,
            OsdiScore: s.OsdiScore,
            OsdiSeverity: s.OsdiSeverity,
            ClinicalNotes: s.ClinicalNotes,
            PerformedById: s.PerformedById,
            PerformedByName: null, // Would need cross-module query; not needed for approval queue
            VisitId: s.VisitId,
            ScheduledAt: s.ScheduledAt,
            CompletedAt: s.CompletedAt,
            CreatedAt: s.CreatedAt,
            IntervalOverrideReason: s.IntervalOverrideReason,
            Consumables: s.Consumables.Select(c => new SessionConsumableDto(
                Id: c.Id,
                ConsumableItemId: c.ConsumableItemId,
                ConsumableName: c.ConsumableName,
                Quantity: c.Quantity)).ToList()
        )).ToList();

        CancellationRequestDto? cancellationRequestDto = null;
        if (package.CancellationRequest is not null)
        {
            var cr = package.CancellationRequest;
            cancellationRequestDto = new CancellationRequestDto(
                Id: cr.Id,
                RequestedById: cr.RequestedById,
                RequestedByName: string.Empty, // Would need cross-module query
                RequestedAt: cr.RequestedAt,
                Reason: cr.Reason,
                DeductionPercent: cr.DeductionPercent,
                RefundAmount: cr.RefundAmount,
                Status: cr.Status.ToString(),
                ApprovedById: cr.ProcessedById,
                ApprovedByName: null,
                ApprovedAt: cr.ProcessedAt,
                RejectionReason: cr.ProcessingNote);
        }

        // Compute last session date and next due date
        var lastSessionDate = package.Sessions
            .Where(s => s.Status == Treatment.Domain.Enums.SessionStatus.Completed)
            .MaxBy(s => s.CompletedAt)?.CompletedAt;

        DateTime? nextDueDate = lastSessionDate.HasValue && package.SessionsRemaining > 0
            ? lastSessionDate.Value.AddDays(package.MinIntervalDays)
            : null;

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
            Sessions: sessions,
            CancellationRequest: cancellationRequestDto);
    }
}
