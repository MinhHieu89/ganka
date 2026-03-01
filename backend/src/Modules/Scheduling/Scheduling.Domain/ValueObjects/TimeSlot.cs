using Shared.Domain;

namespace Scheduling.Domain.ValueObjects;

/// <summary>
/// Value object representing a time slot with start and end times.
/// Validates that End is after Start.
/// </summary>
public class TimeSlot : ValueObject
{
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    public int DurationMinutes => (int)(End - Start).TotalMinutes;

    private TimeSlot() { }

    public TimeSlot(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("End time must be after start time.");

        Start = start;
        End = end;
    }

    /// <summary>
    /// Checks if this time slot overlaps with another.
    /// Two slots overlap if one starts before the other ends and ends after the other starts.
    /// </summary>
    public bool OverlapsWith(TimeSlot other)
    {
        return Start < other.End && End > other.Start;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
