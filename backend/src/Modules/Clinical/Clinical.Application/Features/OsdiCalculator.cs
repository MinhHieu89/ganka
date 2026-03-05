using Clinical.Domain.Enums;

namespace Clinical.Application.Features;

/// <summary>
/// OSDI (Ocular Surface Disease Index) score calculator.
/// Formula: (sum of answers * 100) / (questions answered * 4)
/// Severity: Normal 0-12, Mild 13-22, Moderate 23-32, Severe 33-100.
/// </summary>
public static class OsdiCalculator
{
    /// <summary>
    /// Calculates the OSDI score and severity from an array of answers (0-4 each, null for unanswered).
    /// Returns null if no questions were answered (division-by-zero guard).
    /// </summary>
    public static OsdiResult? Calculate(int?[] answers)
    {
        var answered = answers.Where(a => a.HasValue).ToList();
        if (answered.Count == 0)
            return null;

        var sum = answered.Sum(a => a!.Value);
        var score = Math.Round((decimal)(sum * 100) / (answered.Count * 4), 2);

        var severity = score switch
        {
            <= 12m => OsdiSeverity.Normal,
            <= 22m => OsdiSeverity.Mild,
            <= 32m => OsdiSeverity.Moderate,
            _ => OsdiSeverity.Severe
        };

        return new OsdiResult(score, severity, answered.Count);
    }
}

/// <summary>
/// Result of the OSDI score calculation.
/// </summary>
public sealed record OsdiResult(decimal Score, OsdiSeverity Severity, int AnsweredCount);
