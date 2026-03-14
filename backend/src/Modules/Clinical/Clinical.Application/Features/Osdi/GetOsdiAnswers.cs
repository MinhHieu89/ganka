using System.Text.Json;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for retrieving structured OSDI answers grouped by category.
/// Maps the 12 OSDI question answers to their bilingual question text
/// and groups them: Vision (Q1-Q5), Eye Symptoms (Q6-Q9), Environmental Triggers (Q10-Q12).
/// </summary>
public static class GetOsdiAnswersHandler
{
    /// <summary>
    /// The 12 OSDI questions with bilingual text, reused from GetOsdiByTokenHandler.
    /// </summary>
    private static readonly (int Number, string TextEn, string TextVi, string Category)[] OsdiQuestions =
    [
        (1, "Eyes that are sensitive to light?", "M\u1eaft nh\u1ea1y c\u1ea3m v\u1edbi \u00e1nh s\u00e1ng?", "Ocular Symptoms"),
        (2, "Eyes that feel gritty?", "M\u1eaft c\u1ea3m th\u1ea5y nh\u01b0 c\u00f3 c\u00e1t?", "Ocular Symptoms"),
        (3, "Painful or sore eyes?", "M\u1eaft \u0111au ho\u1eb7c r\u00e1t?", "Ocular Symptoms"),
        (4, "Blurred vision?", "Nh\u00ecn m\u1edd?", "Ocular Symptoms"),
        (5, "Poor vision?", "Th\u1ecb l\u1ef1c k\u00e9m?", "Ocular Symptoms"),
        (6, "Reading?", "\u0110\u1ecdc s\u00e1ch?", "Vision-Related Function"),
        (7, "Driving at night?", "L\u00e1i xe ban \u0111\u00eam?", "Vision-Related Function"),
        (8, "Working with a computer or bank machine (ATM)?", "L\u00e0m vi\u1ec7c v\u1edbi m\u00e1y t\u00ednh ho\u1eb7c m\u00e1y ATM?", "Vision-Related Function"),
        (9, "Watching TV?", "Xem ti vi?", "Vision-Related Function"),
        (10, "Windy conditions?", "\u0110i\u1ec1u ki\u1ec7n gi\u00f3?", "Environmental Triggers"),
        (11, "Places or areas with low humidity (very dry)?", "N\u01a1i c\u00f3 \u0111\u1ed9 \u1ea9m th\u1ea5p (r\u1ea5t kh\u00f4)?", "Environmental Triggers"),
        (12, "Areas that are air conditioned?", "Khu v\u1ef1c c\u00f3 m\u00e1y l\u1ea1nh?", "Environmental Triggers"),
    ];

    public static async Task<OsdiAnswersResponse?> Handle(
        GetOsdiAnswersQuery query,
        IOsdiSubmissionRepository osdiRepository,
        CancellationToken ct)
    {
        var submission = await osdiRepository.GetByVisitIdAsync(query.VisitId, ct);
        if (submission is null)
            return null;

        // Parse answers JSON
        int?[] answers;
        try
        {
            answers = JsonSerializer.Deserialize<int?[]>(submission.AnswersJson) ?? [];
        }
        catch
        {
            answers = [];
        }

        // Build grouped answer list
        var groups = OsdiQuestions
            .GroupBy(q => q.Category)
            .Select(g => new OsdiAnswerGroup(
                g.Key,
                g.Select(q => new OsdiQuestionAnswer(
                    q.Number,
                    q.TextEn,
                    q.TextVi,
                    q.Number - 1 < answers.Length ? answers[q.Number - 1] : null
                )).ToList()
            )).ToList();

        var severityText = submission.Severity.ToString();

        return new OsdiAnswersResponse(groups, submission.Score, severityText);
    }
}
