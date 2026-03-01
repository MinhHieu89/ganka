using Shared.Domain;

namespace Scheduling.Domain.Entities;

/// <summary>
/// Represents the clinic operating schedule for a specific day of the week.
/// Used to validate that appointments fall within operating hours.
/// </summary>
public class ClinicSchedule : Entity
{
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsOpen { get; private set; }
    public TimeSpan? OpenTime { get; private set; }
    public TimeSpan? CloseTime { get; private set; }
    public BranchId BranchId { get; private set; }

    private ClinicSchedule() { }

    public ClinicSchedule(DayOfWeek dayOfWeek, bool isOpen, TimeSpan? openTime, TimeSpan? closeTime, BranchId branchId)
    {
        DayOfWeek = dayOfWeek;
        IsOpen = isOpen;
        OpenTime = openTime;
        CloseTime = closeTime;
        BranchId = branchId;
    }

    /// <summary>
    /// Validates that both start and end times fall within operating hours.
    /// Returns false if the clinic is closed on this day.
    /// </summary>
    public bool IsWithinHours(TimeSpan startTime, TimeSpan endTime)
    {
        if (!IsOpen || OpenTime is null || CloseTime is null)
            return false;

        return startTime >= OpenTime.Value && endTime <= CloseTime.Value;
    }
}
