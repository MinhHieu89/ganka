using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Technician work order within the clinical workflow.
/// Created when a visit enters PreExam stage or when a doctor requests additional exams.
/// Tracks assignment, completion, and red-flag status.
/// </summary>
public class TechnicianOrder : Entity, IAuditable
{
    public Guid VisitId { get; private set; }
    public TechnicianOrderType OrderType { get; private set; }
    public Guid? TechnicianId { get; private set; }
    public string? TechnicianName { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool IsRedFlag { get; private set; }
    public string? RedFlagReason { get; private set; }
    public DateTime? RedFlaggedAt { get; private set; }
    public Guid? OrderedByDoctorId { get; private set; }
    public string? OrderedByDoctorName { get; private set; }
    public string? Instructions { get; private set; }
    public DateTime OrderedAt { get; private set; }

    private TechnicianOrder() { }

    /// <summary>
    /// Creates a PreExam technician order for a visit.
    /// </summary>
    public static TechnicianOrder CreatePreExam(Guid visitId)
    {
        return new TechnicianOrder
        {
            VisitId = visitId,
            OrderType = TechnicianOrderType.PreExam,
            OrderedAt = DateTime.UtcNow,
            IsRedFlag = false
        };
    }

    /// <summary>
    /// Accepts the order by a technician. Throws if already accepted.
    /// </summary>
    public void Accept(Guid technicianId, string technicianName)
    {
        if (TechnicianId.HasValue)
            throw new InvalidOperationException(
                $"Order already accepted by {TechnicianName}.");

        TechnicianId = technicianId;
        TechnicianName = technicianName;
        StartedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Completes the order. Must be accepted first.
    /// </summary>
    public void Complete()
    {
        if (!TechnicianId.HasValue)
            throw new InvalidOperationException(
                "Cannot complete an order that has not been accepted.");

        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Returns the order to the queue by clearing technician assignment.
    /// </summary>
    public void ReturnToQueue()
    {
        TechnicianId = null;
        TechnicianName = null;
        StartedAt = null;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the order with a red flag and completes it.
    /// </summary>
    public void MarkRedFlag(string reason)
    {
        IsRedFlag = true;
        RedFlagReason = reason;
        RedFlaggedAt = DateTime.UtcNow;
        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
