namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for clinic operating schedule (one entry per day of week).
/// </summary>
public record ClinicScheduleDto(
    DayOfWeek DayOfWeek,
    bool IsOpen,
    TimeSpan? OpenTime,
    TimeSpan? CloseTime);
