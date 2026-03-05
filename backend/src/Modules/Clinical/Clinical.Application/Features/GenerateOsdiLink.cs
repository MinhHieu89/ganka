using System.Security.Cryptography;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for generating a public OSDI questionnaire link for patient self-fill.
/// Creates an OsdiSubmission with a cryptographically secure token and 24-hour expiry.
/// </summary>
public static class GenerateOsdiLinkHandler
{
    public static async Task<Result<OsdiLinkResponse>> Handle(
        GenerateOsdiLinkCommand command,
        IVisitRepository visitRepository,
        IOsdiSubmissionRepository osdiRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result<OsdiLinkResponse>.Failure(Error.NotFound("Visit", command.VisitId));

        // Generate cryptographically secure token (32 bytes -> base64)
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('='); // URL-safe base64

        var submission = OsdiSubmission.CreateWithToken(command.VisitId, token);
        await osdiRepository.AddAsync(submission, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var url = $"/osdi/{token}";
        var expiresAt = submission.TokenExpiresAt!.Value;

        return new OsdiLinkResponse(token, url, expiresAt);
    }
}
