namespace Clinical.Contracts.Dtos;

/// <summary>
/// Response containing structured OSDI answers grouped by category.
/// </summary>
public sealed record OsdiAnswersResponse(
    List<OsdiAnswerGroup> Groups,
    decimal TotalScore,
    string Severity);

/// <summary>
/// A group of OSDI answers within a category (Vision, Eye Symptoms, Environmental Triggers).
/// </summary>
public sealed record OsdiAnswerGroup(
    string Category,
    List<OsdiQuestionAnswer> Questions);

/// <summary>
/// A single OSDI question with its answer score and bilingual question text.
/// </summary>
public sealed record OsdiQuestionAnswer(
    int QuestionNumber,
    string TextEn,
    string TextVi,
    int? Score);

/// <summary>
/// Query for OSDI answers for a specific visit.
/// </summary>
public sealed record GetOsdiAnswersQuery(Guid VisitId);
