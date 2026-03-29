using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for technician KPI stats.
/// Queries today's TechnicianOrders and returns aggregated counts per D-09.
/// </summary>
public static class GetTechnicianKpiStatsHandler
{
    public static async Task<TechnicianKpiDto> Handle(
        GetTechnicianKpiQuery query,
        ITechnicianOrderQueryService queryService,
        CancellationToken ct)
    {
        var todayOrders = await queryService.GetTodayOrderSummariesAsync(ct);

        var waiting = todayOrders.Count(o => !o.TechnicianId.HasValue && !o.CompletedAt.HasValue);
        var inProgress = todayOrders.Count(o => o.TechnicianId == query.CurrentTechnicianId && !o.CompletedAt.HasValue);
        var completed = todayOrders.Count(o => o.CompletedAt.HasValue && !o.IsRedFlag);
        var redFlag = todayOrders.Count(o => o.IsRedFlag);

        return new TechnicianKpiDto(waiting, inProgress, completed, redFlag);
    }
}
