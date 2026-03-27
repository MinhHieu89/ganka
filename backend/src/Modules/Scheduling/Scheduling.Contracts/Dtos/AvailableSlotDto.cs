namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for a time slot with availability information (D-10).
/// </summary>
public sealed record AvailableSlotDto(
    DateTime StartTime,
    DateTime EndTime,
    bool IsAvailable,
    int BookedCount);
