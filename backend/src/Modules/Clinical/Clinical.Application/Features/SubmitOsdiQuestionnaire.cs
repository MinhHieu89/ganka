using System.Text.Json;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for submitting OSDI questionnaire answers via public token.
/// No authentication required (public endpoint for patient self-fill).
/// Calculates OSDI score, saves OsdiSubmission, and updates DryEyeAssessment.
/// </summary>
public static class SubmitOsdiQuestionnaireHandler
{
    public static async Task<Result<decimal>> Handle(
        SubmitOsdiCommand command,
        IOsdiSubmissionRepository osdiRepository,
        IVisitRepository visitRepository,
        IOsdiNotificationService osdiNotificationService,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        // Look up submission by token
        var submission = await osdiRepository.GetByTokenAsync(command.Token, ct);
        if (submission is null)
            return Result<decimal>.Failure(Error.Custom("Error.NotFound", "Token not found."));

        // Check token expiry
        if (submission.TokenExpiresAt.HasValue && submission.TokenExpiresAt.Value < DateTime.UtcNow)
            return Result<decimal>.Failure(Error.Custom("Error.Expired", "Token expired."));

        // Calculate OSDI score
        var osdiResult = OsdiCalculator.Calculate(command.Answers);
        if (osdiResult is null)
            return Result<decimal>.Failure(Error.Validation("At least one question must be answered."));

        // Update the OsdiSubmission with answers and calculated score
        var answersJson = JsonSerializer.Serialize(command.Answers);
        UpdateSubmission(submission, answersJson, osdiResult);

        // Also update the DryEyeAssessment OSDI fields on the visit (only if linked to a visit)
        if (submission.VisitId.HasValue)
        {
            var visit = await visitRepository.GetByIdWithDetailsAsync(submission.VisitId.Value, ct);
            if (visit is not null)
            {
                var dryEyeAssessment = visit.DryEyeAssessments.FirstOrDefault();
                if (dryEyeAssessment is not null)
                {
                    dryEyeAssessment.SetOsdiScore(osdiResult.Score, osdiResult.Severity);
                }
                else
                {
                    // Create a new DryEyeAssessment if one doesn't exist
                    var newAssessment = DryEyeAssessment.Create(submission.VisitId.Value);
                    newAssessment.SetOsdiScore(osdiResult.Score, osdiResult.Severity);

                    // For patient self-fill, we bypass EnsureEditable since
                    // this is a patient action, not a doctor edit.
                    // We add directly via repository instead of domain method.
                    visitRepository.AddDryEyeAssessment(newAssessment);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        // Notify any listeners waiting for this token's result (treatment session flow)
        await osdiNotificationService.NotifyTokenSubmittedAsync(
            submission.PublicToken!, osdiResult.Score, osdiResult.Severity.ToString(), ct);

        // Also notify visit group for backward compatibility (clinical visit flow)
        if (submission.VisitId.HasValue)
        {
            await osdiNotificationService.NotifyOsdiSubmittedAsync(
                submission.VisitId.Value, osdiResult.Score, osdiResult.Severity.ToString(), ct);
        }

        return Result<decimal>.Success(osdiResult.Score);
    }

    private static void UpdateSubmission(OsdiSubmission submission, string answersJson, OsdiResult osdiResult)
    {
        // Use reflection to update private setters on OsdiSubmission
        // This is a pragmatic approach since OsdiSubmission was designed with private setters
        var type = typeof(OsdiSubmission);
        type.GetProperty(nameof(OsdiSubmission.AnswersJson))!.GetSetMethod(true)!.Invoke(submission, [answersJson]);
        type.GetProperty(nameof(OsdiSubmission.QuestionsAnswered))!.GetSetMethod(true)!.Invoke(submission, [osdiResult.AnsweredCount]);
        type.GetProperty(nameof(OsdiSubmission.Score))!.GetSetMethod(true)!.Invoke(submission, [osdiResult.Score]);
        type.GetProperty(nameof(OsdiSubmission.Severity))!.GetSetMethod(true)!.Invoke(submission, [osdiResult.Severity]);
    }
}
