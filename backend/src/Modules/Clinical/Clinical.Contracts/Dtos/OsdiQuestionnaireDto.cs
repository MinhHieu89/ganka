namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for the full OSDI questionnaire with 12 questions in Vietnamese and English.
/// Used for both doctor-recorded and patient self-fill modes.
/// </summary>
public sealed record OsdiQuestionnaireDto(
    List<OsdiQuestionDto> Questions,
    int[]? CurrentAnswers,
    DateTime VisitDate);

/// <summary>
/// A single OSDI question with bilingual text.
/// </summary>
public sealed record OsdiQuestionDto(
    int Index,
    string TextEn,
    string TextVi);

/// <summary>
/// Command to submit OSDI answers via public token (patient self-fill).
/// </summary>
public sealed record SubmitOsdiCommand(
    string Token,
    int?[] Answers);

/// <summary>
/// Command to generate a public OSDI questionnaire link for patient self-fill.
/// </summary>
public sealed record GenerateOsdiLinkCommand(
    Guid VisitId);

/// <summary>
/// Response containing the generated OSDI link details.
/// </summary>
public sealed record OsdiLinkResponse(
    string Token,
    string Url,
    DateTime ExpiresAt);
