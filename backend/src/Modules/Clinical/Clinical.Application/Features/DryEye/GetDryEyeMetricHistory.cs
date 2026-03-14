using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for querying per-metric dry eye time-series data.
/// Returns 5 metric time series (TBUT, Schirmer, MeibomianGrading, TearMeniscus, StainingScore)
/// with OD/OS values per visit, supporting time range filtering.
/// </summary>
public static class GetDryEyeMetricHistoryHandler
{
    private static readonly string[] MetricNames =
        ["TBUT", "Schirmer", "MeibomianGrading", "TearMeniscus", "StainingScore"];

    public static async Task<DryEyeMetricHistoryResponse> Handle(
        GetDryEyeMetricHistoryQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        var since = GetCutoffDate(query.TimeRange);
        var history = await visitRepository.GetMetricHistoryAsync(query.PatientId, since, ct);

        var metrics = MetricNames.Select(name => new MetricTimeSeries(
            name,
            history.Select(h => new MetricDataPoint(
                h.VisitDate,
                GetOdValue(h.Assessment, name),
                GetOsValue(h.Assessment, name)
            )).ToList()
        )).ToList();

        return new DryEyeMetricHistoryResponse(metrics);
    }

    private static DateTime? GetCutoffDate(string timeRange) => timeRange switch
    {
        "3m" => DateTime.UtcNow.AddMonths(-3),
        "6m" => DateTime.UtcNow.AddMonths(-6),
        "1y" => DateTime.UtcNow.AddYears(-1),
        _ => null
    };

    private static decimal? GetOdValue(Domain.Entities.DryEyeAssessment a, string metric) => metric switch
    {
        "TBUT" => a.OdTbut,
        "Schirmer" => a.OdSchirmer,
        "MeibomianGrading" => a.OdMeibomianGrading,
        "TearMeniscus" => a.OdTearMeniscus,
        "StainingScore" => a.OdStaining,
        _ => null
    };

    private static decimal? GetOsValue(Domain.Entities.DryEyeAssessment a, string metric) => metric switch
    {
        "TBUT" => a.OsTbut,
        "Schirmer" => a.OsSchirmer,
        "MeibomianGrading" => a.OsMeibomianGrading,
        "TearMeniscus" => a.OsTearMeniscus,
        "StainingScore" => a.OsStaining,
        _ => null
    };
}
