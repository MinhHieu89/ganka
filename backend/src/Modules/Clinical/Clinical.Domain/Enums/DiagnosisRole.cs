namespace Clinical.Domain.Enums;

/// <summary>
/// Role of a diagnosis within a visit.
/// First diagnosis is Primary, subsequent ones are Secondary.
/// </summary>
public enum DiagnosisRole
{
    Primary = 0,
    Secondary = 1
}
