using Clinical.Application.Interfaces;
using Clinical.Contracts.IntegrationEvents;
using Clinical.Domain.Entities;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for cross-module command from Treatment module.
/// Creates a DB-backed OsdiSubmission with a public token (no VisitId required).
/// This allows the public OSDI page to find the token via GetOsdiByToken.
/// </summary>
public static class CreateOsdiTokenForTreatmentHandler
{
    public static async Task<CreateOsdiTokenForTreatmentResponse> Handle(
        CreateOsdiTokenForTreatmentCommand command,
        IOsdiSubmissionRepository osdiRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var submission = OsdiSubmission.CreateWithTokenForTreatment(command.Token);
        await osdiRepository.AddAsync(submission, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var url = $"/osdi/{command.Token}";
        var expiresAt = submission.TokenExpiresAt!.Value;

        return new CreateOsdiTokenForTreatmentResponse(command.Token, url, expiresAt);
    }
}
