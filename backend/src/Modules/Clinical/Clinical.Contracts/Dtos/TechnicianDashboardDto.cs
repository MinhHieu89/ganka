namespace Clinical.Contracts.Dtos;

/// <summary>
/// Query to fetch the technician dashboard data.
/// CurrentTechnicianId is enriched from JWT claims at the endpoint level.
/// </summary>
public record GetTechnicianDashboardQuery(
    string? Status,
    string? Search,
    int Page = 1,
    int PageSize = 50,
    Guid? CurrentTechnicianId = null);

/// <summary>
/// Dashboard response containing paged technician order rows.
/// </summary>
public record TechnicianDashboardDto(List<TechnicianDashboardRowDto> Items, int TotalCount);

/// <summary>
/// A single row in the technician dashboard table.
/// Status is derived from TechnicianOrder fields per D-08.
/// VisitType is derived from patient visit history per D-10.
/// </summary>
public record TechnicianDashboardRowDto(
    Guid OrderId,
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    string? PatientCode,
    int? BirthYear,
    DateTime CheckinTime,
    int WaitMinutes,
    string? Reason,
    string VisitType,    // "new", "follow_up", "additional"
    string Status,       // "waiting", "in_progress", "red_flag", "completed"
    string? TechnicianName,
    string? RedFlagReason,
    bool IsRedFlag);
