using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Query to generate a discrepancy report for a completed stocktaking session.
/// </summary>
public sealed record GetDiscrepancyReportQuery(Guid SessionId);

/// <summary>
/// Wolverine static handler for generating a discrepancy report from a stocktaking session.
/// Returns a summary with counts for over, under, and missing-from-system items.
/// Returns failure if session not found.
/// </summary>
public static class GetDiscrepancyReportHandler
{
    public static async Task<Result<DiscrepancyReportDto>> Handle(
        GetDiscrepancyReportQuery query,
        IStocktakingRepository repository,
        CancellationToken ct)
    {
        var session = await repository.GetByIdAsync(query.SessionId, ct);
        if (session is null)
            return Result.Failure<DiscrepancyReportDto>(
                Error.NotFound("StocktakingSession", query.SessionId));

        var items = session.Items;

        var itemDtos = items.Select(i => new StocktakingItemDto(
            Id: i.Id,
            StocktakingSessionId: i.StocktakingSessionId,
            Barcode: i.Barcode,
            FrameId: i.FrameId,
            FrameName: i.FrameName,
            PhysicalCount: i.PhysicalCount,
            SystemCount: i.SystemCount,
            Discrepancy: i.Discrepancy)).ToList();

        int totalDiscrepancies = items.Count(i => i.Discrepancy != 0);
        int overCount = items.Count(i => i.Discrepancy > 0);
        int underCount = items.Count(i => i.Discrepancy < 0 && i.FrameId is not null);
        int missingFromSystem = items.Count(i => i.Discrepancy > 0 && i.FrameId is null);

        var report = new DiscrepancyReportDto(
            SessionId: session.Id,
            SessionName: session.Name,
            CompletedAt: session.CompletedAt,
            TotalScanned: items.Count,
            TotalDiscrepancies: totalDiscrepancies,
            OverCount: overCount,
            UnderCount: underCount,
            MissingFromSystem: missingFromSystem,
            Items: itemDtos);

        return Result.Success(report);
    }
}
