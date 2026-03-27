namespace Scheduling.Contracts.Queries;

/// <summary>
/// Query to get the receptionist dashboard with filtering and pagination.
/// </summary>
public record GetReceptionistDashboardQuery(
    string? StatusFilter,
    string? Search,
    int Page = 1,
    int PageSize = 50);

/// <summary>
/// Query to get receptionist KPI stats for today.
/// </summary>
public record GetReceptionistKpiStatsQuery();
