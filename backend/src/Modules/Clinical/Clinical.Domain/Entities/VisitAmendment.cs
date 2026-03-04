using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Represents an amendment to a signed visit.
/// Captures field-level diffs with mandatory reason for audit trail.
/// </summary>
public class VisitAmendment : Entity, IAuditable
{
    public Guid VisitId { get; private set; }
    public Guid AmendedById { get; private set; }
    public string AmendedByName { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string FieldChangesJson { get; private set; } = string.Empty;
    public DateTime AmendedAt { get; private set; }

    private VisitAmendment() { }

    /// <summary>
    /// Factory method for creating a new amendment record.
    /// </summary>
    public static VisitAmendment Create(
        Guid visitId,
        Guid amendedById,
        string amendedByName,
        string reason,
        string fieldChangesJson)
    {
        return new VisitAmendment
        {
            VisitId = visitId,
            AmendedById = amendedById,
            AmendedByName = amendedByName,
            Reason = reason,
            FieldChangesJson = fieldChangesJson,
            AmendedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Represents a single field-level change within an amendment.
/// Serialized to JSON in VisitAmendment.FieldChangesJson.
/// </summary>
public record FieldChange(string FieldName, string? OldValue, string? NewValue);
