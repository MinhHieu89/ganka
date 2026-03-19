using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// OSDI questionnaire submission. Stores the 12 answers and calculated score.
/// Supports both doctor-recorded and patient self-fill via public token.
/// The public token enables unauthenticated patient access with 24-hour expiry.
/// </summary>
public class OsdiSubmission : Entity
{
    public Guid? VisitId { get; private set; }

    /// <summary>
    /// Who submitted: "patient" for self-fill, or a userId string for staff-recorded.
    /// </summary>
    public string SubmittedBy { get; private set; } = string.Empty;

    /// <summary>
    /// JSON array of 12 answers (each 0-4 or null for unanswered).
    /// </summary>
    public string AnswersJson { get; private set; } = "[]";

    /// <summary>
    /// Number of questions answered (0-12). Used in OSDI score calculation.
    /// </summary>
    public int QuestionsAnswered { get; private set; }

    /// <summary>
    /// Calculated OSDI score (0-100). Formula: (sum of answers / (questions answered * 4)) * 100.
    /// </summary>
    public decimal Score { get; private set; }

    /// <summary>
    /// Calculated severity from Score. Normal 0-12, Mild 13-22, Moderate 23-32, Severe 33-100.
    /// </summary>
    public OsdiSeverity Severity { get; private set; }

    /// <summary>
    /// Public token for patient self-fill. Null if staff-recorded.
    /// </summary>
    public string? PublicToken { get; private set; }

    /// <summary>
    /// When the public token expires (24 hours after creation). Null if staff-recorded.
    /// </summary>
    public DateTime? TokenExpiresAt { get; private set; }

    private OsdiSubmission() { }

    /// <summary>
    /// Factory method for creating a staff-recorded OSDI submission.
    /// </summary>
    public static OsdiSubmission Create(
        Guid visitId,
        string submittedBy,
        string answersJson,
        int questionsAnswered,
        decimal score,
        OsdiSeverity severity)
    {
        return new OsdiSubmission
        {
            VisitId = visitId,
            SubmittedBy = submittedBy,
            AnswersJson = answersJson,
            QuestionsAnswered = questionsAnswered,
            Score = score,
            Severity = severity
        };
    }

    /// <summary>
    /// Factory method for creating an OSDI submission with a public token for patient self-fill.
    /// Token expires in 24 hours. Used by Clinical module's visit-based flow.
    /// </summary>
    public static OsdiSubmission CreateWithToken(
        Guid visitId,
        string token)
    {
        return new OsdiSubmission
        {
            VisitId = visitId,
            SubmittedBy = "patient",
            PublicToken = token,
            TokenExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    /// <summary>
    /// Factory method for creating an OSDI submission with a public token for treatment session flow.
    /// No VisitId required — the token is created independently of a clinical visit.
    /// Token expires in 24 hours.
    /// </summary>
    public static OsdiSubmission CreateWithTokenForTreatment(string token)
    {
        return new OsdiSubmission
        {
            VisitId = null,
            SubmittedBy = "patient",
            PublicToken = token,
            TokenExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }
}
