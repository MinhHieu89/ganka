using Optical.Application.Interfaces;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Application.Features.Warranty;

/// <summary>
/// Command for manager to approve or reject a warranty claim (Replace resolution only).
/// Only claims with WarrantyResolution.Replace require manager approval.
/// </summary>
public sealed record ApproveWarrantyClaimCommand(Guid ClaimId, bool IsApproved, string? Notes);

/// <summary>
/// Wolverine static handler for manager approval/rejection of warranty claims.
/// Only Replace resolution claims require this step.
/// Repair and Discount claims are auto-approved at creation.
/// </summary>
public static class ApproveWarrantyClaimHandler
{
    public static async Task<Result> Handle(
        ApproveWarrantyClaimCommand command,
        IWarrantyClaimRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var claim = await repository.GetByIdAsync(command.ClaimId, ct);
        if (claim is null)
            return Result.Failure(Error.NotFound("WarrantyClaim", command.ClaimId));

        // Only Replace resolution requires manager approval
        if (claim.Resolution != WarrantyResolution.Replace)
            return Result.Failure(Error.Validation(
                "Only warranty claims with Replace resolution require manager approval. " +
                "Repair and Discount claims are automatically approved."));

        if (!command.IsApproved)
        {
            // Rejection requires a reason
            if (string.IsNullOrWhiteSpace(command.Notes))
                return Result.Failure(Error.Validation(
                    "A rejection reason is required when rejecting a warranty claim."));

            claim.Reject(currentUser.UserId, command.Notes);
        }
        else
        {
            claim.Approve(currentUser.UserId);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
