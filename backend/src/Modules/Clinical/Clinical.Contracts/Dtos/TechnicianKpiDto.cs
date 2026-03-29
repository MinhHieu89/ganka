namespace Clinical.Contracts.Dtos;

/// <summary>
/// Query to fetch technician KPI stats for today.
/// CurrentTechnicianId is enriched from JWT claims at the endpoint level.
/// </summary>
public record GetTechnicianKpiQuery(Guid CurrentTechnicianId);

/// <summary>
/// KPI stats for the technician dashboard header cards.
/// </summary>
public record TechnicianKpiDto(
    int Waiting,
    int InProgress,
    int Completed,
    int RedFlag);
