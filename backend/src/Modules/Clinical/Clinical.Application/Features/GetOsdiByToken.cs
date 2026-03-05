using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for retrieving the OSDI questionnaire via public token.
/// Returns the 12 OSDI questions with Vietnamese and English text.
/// Returns minimal data (questions + visit date only) for security.
/// </summary>
public static class GetOsdiByTokenHandler
{
    /// <summary>
    /// The 12 OSDI questions with bilingual text.
    /// </summary>
    private static readonly List<OsdiQuestionDto> OsdiQuestions =
    [
        new(1, "Eyes that are sensitive to light?", "M\u1eaft nh\u1ea1y c\u1ea3m v\u1edbi \u00e1nh s\u00e1ng?"),
        new(2, "Eyes that feel gritty?", "M\u1eaft c\u1ea3m th\u1ea5y nh\u01b0 c\u00f3 c\u00e1t?"),
        new(3, "Painful or sore eyes?", "M\u1eaft \u0111au ho\u1eb7c r\u00e1t?"),
        new(4, "Blurred vision?", "Nh\u00ecn m\u1edd?"),
        new(5, "Poor vision?", "Th\u1ecb l\u1ef1c k\u00e9m?"),
        new(6, "Reading?", "\u0110\u1ecdc s\u00e1ch?"),
        new(7, "Driving at night?", "L\u00e1i xe ban \u0111\u00eam?"),
        new(8, "Working with a computer or bank machine (ATM)?", "L\u00e0m vi\u1ec7c v\u1edbi m\u00e1y t\u00ednh ho\u1eb7c m\u00e1y ATM?"),
        new(9, "Watching TV?", "Xem ti vi?"),
        new(10, "Windy conditions?", "\u0110i\u1ec1u ki\u1ec7n gi\u00f3?"),
        new(11, "Places or areas with low humidity (very dry)?", "N\u01a1i c\u00f3 \u0111\u1ed9 \u1ea9m th\u1ea5p (r\u1ea5t kh\u00f4)?"),
        new(12, "Areas that are air conditioned?", "Khu v\u1ef1c c\u00f3 m\u00e1y l\u1ea1nh?"),
    ];

    public static async Task<Result<OsdiQuestionnaireDto>> Handle(
        GetOsdiByTokenQuery query,
        IOsdiSubmissionRepository osdiRepository,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        var submission = await osdiRepository.GetByTokenAsync(query.Token, ct);
        if (submission is null)
            return Result<OsdiQuestionnaireDto>.Failure(Error.Custom("Error.NotFound", "Token not found."));

        // Check token expiry
        if (submission.TokenExpiresAt.HasValue && submission.TokenExpiresAt.Value < DateTime.UtcNow)
            return Result<OsdiQuestionnaireDto>.Failure(Error.Custom("Error.Expired", "Token expired."));

        // Get visit date for display (minimal data, no patient name for security)
        var visit = await visitRepository.GetByIdAsync(submission.VisitId, ct);
        var visitDate = visit?.VisitDate ?? DateTime.UtcNow;

        // Parse existing answers if any
        int[]? currentAnswers = null;
        if (submission.AnswersJson != "[]" && !string.IsNullOrEmpty(submission.AnswersJson))
        {
            try
            {
                currentAnswers = System.Text.Json.JsonSerializer.Deserialize<int[]>(submission.AnswersJson);
            }
            catch
            {
                // Ignore parse errors, return null for answers
            }
        }

        return new OsdiQuestionnaireDto(OsdiQuestions, currentAnswers, visitDate);
    }
}
