namespace Clinical.Domain.Enums;

/// <summary>
/// Status of a clinical visit record.
/// Draft = editable, Signed = immutable, Amended = editable again via amendment workflow.
/// </summary>
public enum VisitStatus
{
    Draft = 0,
    Signed = 1,
    Amended = 2
}
