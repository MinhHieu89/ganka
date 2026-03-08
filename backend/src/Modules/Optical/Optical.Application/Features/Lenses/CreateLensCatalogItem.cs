using FluentValidation;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Application.Features.Lenses;

/// <summary>
/// Command to create a new lens catalog item.
/// </summary>
public sealed record CreateLensCatalogItemCommand(
    string Brand,
    string Name,
    string LensType,
    int Material,
    int AvailableCoatings,
    decimal SellingPrice,
    decimal CostPrice,
    Guid? PreferredSupplierId);

/// <summary>
/// Validates <see cref="CreateLensCatalogItemCommand"/> fields.
/// LensType must be one of: single_vision, bifocal, progressive, reading.
/// Prices must be >= 0.
/// </summary>
public class CreateLensCatalogItemCommandValidator : AbstractValidator<CreateLensCatalogItemCommand>
{
    private static readonly string[] AllowedLensTypes = ["single_vision", "bifocal", "progressive", "reading"];

    public CreateLensCatalogItemCommandValidator()
    {
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.LensType)
            .NotEmpty().WithMessage("LensType is required.")
            .Must(t => AllowedLensTypes.Contains(t))
            .WithMessage($"LensType must be one of: {string.Join(", ", AllowedLensTypes)}.");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price must be >= 0.");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be >= 0.");
    }
}

/// <summary>
/// Wolverine static handler for creating a new lens catalog item.
/// Validates input, creates entity via factory method, persists via repository.
/// </summary>
public static class CreateLensCatalogItemHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateLensCatalogItemCommand command,
        ILensCatalogRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateLensCatalogItemCommand> validator,
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

        var item = LensCatalogItem.Create(
            brand: command.Brand,
            name: command.Name,
            lensType: command.LensType,
            material: (LensMaterial)command.Material,
            availableCoatings: (LensCoating)command.AvailableCoatings,
            sellingPrice: command.SellingPrice,
            costPrice: command.CostPrice,
            supplierId: command.PreferredSupplierId,
            branchId: new BranchId(currentUser.BranchId));

        repository.Add(item);
        await unitOfWork.SaveChangesAsync(ct);

        return item.Id;
    }
}
