using Shared.Domain;

namespace Optical.Application.Features.Warranty;

/// <summary>
/// Command for manager to approve or reject a warranty claim (Replace resolution only).
/// Handler implementation provided in plan 08-19.
/// </summary>
public sealed record ApproveWarrantyClaimCommand(Guid ClaimId, bool IsApproved, string? Notes);
