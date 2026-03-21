namespace Scheduling.Contracts.Queries;

/// <summary>
/// Cross-module query to retrieve the count of today's non-cancelled appointments.
/// Handled by Scheduling.Application.
/// </summary>
public sealed record GetTodayAppointmentCountQuery;
