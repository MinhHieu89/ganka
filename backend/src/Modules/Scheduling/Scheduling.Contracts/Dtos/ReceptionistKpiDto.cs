namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for receptionist KPI stats (counts per status for today).
/// </summary>
public sealed record ReceptionistKpiDto(
    int TodayAppointments,
    int NotArrived,
    int Waiting,
    int Examining,
    int Completed);
