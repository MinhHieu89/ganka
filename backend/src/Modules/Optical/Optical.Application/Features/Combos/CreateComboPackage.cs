using FluentValidation;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Shared.Application;
using Shared.Domain;

namespace Optical.Application.Features.Combos;

/// <summary>
/// Command to create a preset combo package (admin).
/// </summary>
public sealed record CreateComboPackageCommand(
    string Name,
    string? Description,
    Guid? FrameId,
    Guid? LensCatalogItemId,
    decimal ComboPrice,
    decimal? OriginalTotalPrice);

/// <summary>
/// Validator for <see cref="CreateComboPackageCommand"/>.
/// </summary>
public class CreateComboPackageCommandValidator : AbstractValidator<CreateComboPackageCommand>
{
    public CreateComboPackageCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Combo package name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ComboPrice)
            .GreaterThan(0).WithMessage("Combo price must be greater than zero.");

        RuleFor(x => x.OriginalTotalPrice)
            .GreaterThan(0).WithMessage("Original total price must be greater than zero when provided.")
            .When(x => x.OriginalTotalPrice.HasValue);
    }
}

/// <summary>
/// Wolverine static handler for creating a preset combo package.
/// Validates input, creates entity via factory method, persists via repository.
/// </summary>
public static class CreateComboPackageHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateComboPackageCommand command,
        IComboPackageRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateComboPackageCommand> validator,
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

        var combo = ComboPackage.Create(
            command.Name,
            command.Description,
            command.FrameId,
            command.LensCatalogItemId,
            command.ComboPrice,
            command.OriginalTotalPrice,
            new BranchId(currentUser.BranchId));

        repository.Add(combo);
        await unitOfWork.SaveChangesAsync(ct);

        return combo.Id;
    }
}
