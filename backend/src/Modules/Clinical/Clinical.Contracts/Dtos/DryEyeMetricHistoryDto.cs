namespace Clinical.Contracts.Dtos;

/// <summary>
/// Response containing per-metric time-series data for dry eye trend charts.
/// Each metric (TBUT, Schirmer, etc.) has its own time series with OD/OS values.
/// </summary>
public sealed record DryEyeMetricHistoryResponse(
    List<MetricTimeSeries> Metrics);

/// <summary>
/// Time-series data for a single dry eye metric across visits.
/// </summary>
public sealed record MetricTimeSeries(
    string MetricName,
    List<MetricDataPoint> DataPoints);

/// <summary>
/// A single data point in a metric time series with per-eye values.
/// </summary>
public sealed record MetricDataPoint(
    DateTime VisitDate,
    decimal? OdValue,
    decimal? OsValue);

/// <summary>
/// Query for dry eye metric history with time range filtering.
/// TimeRange: "3m" = 3 months, "6m" = 6 months, "1y" = 1 year, "all" = no filter.
/// </summary>
public sealed record GetDryEyeMetricHistoryQuery(
    Guid PatientId,
    string TimeRange = "all");
