namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO representing an available time slot for appointment booking.
/// </summary>
public sealed record AvailableSlotDto(
    DateTime StartTime,
    DateTime EndTime,
    bool IsAvailable,
    int BookedCount);
