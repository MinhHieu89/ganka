namespace Clinical.Domain.Enums;

/// <summary>
/// OSDI (Ocular Surface Disease Index) severity classification.
/// Normal: 0-12, Mild: 13-22, Moderate: 23-32, Severe: 33-100.
/// </summary>
public enum OsdiSeverity
{
    Normal = 0,
    Mild = 1,
    Moderate = 2,
    Severe = 3
}
