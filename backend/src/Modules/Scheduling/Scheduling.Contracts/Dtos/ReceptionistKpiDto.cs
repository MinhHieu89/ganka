namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for receptionist KPI statistics (today's counts by status).
/// </summary>
public sealed record ReceptionistKpiDto(
    int TodayAppointments,
    int NotArrived,
    int Waiting,
    int Examining,
    int Completed,
    int Cancelled);
