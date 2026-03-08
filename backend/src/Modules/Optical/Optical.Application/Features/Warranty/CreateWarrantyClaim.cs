using FluentValidation;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Application.Features.Warranty;

/// <summary>
/// Command to file a new warranty claim for a glasses order.
/// Handler validates order is within 12-month warranty period.
/// For Replace resolution, claim starts as Pending and requires manager approval.
/// For Repair and Discount, claim is auto-approved.
/// </summary>
public sealed record CreateWarrantyClaimCommand(
    Guid GlassesOrderId,
    int Resolution,
    string AssessmentNotes,
    decimal? DiscountAmount);

/// <summary>
/// Validator for <see cref="CreateWarrantyClaimCommand"/>.
/// </summary>
public class CreateWarrantyClaimCommandValidator : AbstractValidator<CreateWarrantyClaimCommand>
{
    public CreateWarrantyClaimCommandValidator()
    {
        RuleFor(x => x.GlassesOrderId)
            .NotEmpty().WithMessage("Glasses order ID is required.");

        RuleFor(x => x.AssessmentNotes)
            .NotEmpty().WithMessage("Assessment notes are required.")
            .MaximumLength(2000).WithMessage("Assessment notes must not exceed 2000 characters.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThan(0).WithMessage("Discount amount must be greater than zero.")
            .When(x => x.DiscountAmount.HasValue);
    }
}

/// <summary>
/// Wolverine static handler for filing a warranty claim.
/// Validates order exists and is within 12-month warranty window.
/// Sets ApprovalStatus based on resolution type.
/// </summary>
public static class CreateWarrantyClaimHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateWarrantyClaimCommand command,
        IGlassesOrderRepository orderRepository,
        IWarrantyClaimRepository warrantyClaimRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateWarrantyClaimCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<Guid>(Error.ValidationWithDetails(errors));
        }

        var order = await orderRepository.GetByIdAsync(command.GlassesOrderId, ct);
        if (order is null)
            return Result.Failure<Guid>(Error.NotFound("GlassesOrder", command.GlassesOrderId));

        if (!order.IsUnderWarranty)
            return Result.Failure<Guid>(Error.Validation(
                "This order is not under warranty. Warranty covers 12 months from the delivery date."));

        var resolution = (WarrantyResolution)command.Resolution;

        var claim = WarrantyClaim.Create(
            glassesOrderId: command.GlassesOrderId,
            claimDate: DateTime.UtcNow,
            resolution: resolution,
            assessmentNotes: command.AssessmentNotes,
            discountAmount: command.DiscountAmount);

        // For Repair and Discount resolutions, auto-approve (no manager needed).
        // For Replace, claim stays as Pending — requires ApproveWarrantyClaim.
        if (resolution != WarrantyResolution.Replace)
        {
            claim.Approve(currentUser.UserId);
        }

        warrantyClaimRepository.Add(claim);
        await unitOfWork.SaveChangesAsync(ct);

        return claim.Id;
    }
}
