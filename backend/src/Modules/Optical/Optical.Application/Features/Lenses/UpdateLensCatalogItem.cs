using FluentValidation;
using Optical.Application.Interfaces;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Application.Features.Lenses;

/// <summary>
/// Command to update an existing lens catalog item.
/// </summary>
public sealed record UpdateLensCatalogItemCommand(
    Guid Id,
    string Brand,
    string Name,
    string LensType,
    int Material,
    int AvailableCoatings,
    decimal SellingPrice,
    decimal CostPrice,
    Guid? PreferredSupplierId,
    bool IsActive);

/// <summary>
/// Validates <see cref="UpdateLensCatalogItemCommand"/> fields.
/// </summary>
public class UpdateLensCatalogItemCommandValidator : AbstractValidator<UpdateLensCatalogItemCommand>
{
    private static readonly string[] AllowedLensTypes = ["single_vision", "bifocal", "progressive", "reading"];

    public UpdateLensCatalogItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("Id is required.");

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
/// Wolverine static handler for updating an existing lens catalog item.
/// Returns NotFound if item does not exist. Updates all fields and persists.
/// </summary>
public static class UpdateLensCatalogItemHandler
{
    public static async Task<Result<bool>> Handle(
        UpdateLensCatalogItemCommand command,
        ILensCatalogRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateLensCatalogItemCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<bool>(Error.ValidationWithDetails(errors));
        }

        var item = await repository.GetByIdAsync(command.Id, ct);
        if (item is null)
            return Result.Failure<bool>(Error.NotFound("LensCatalogItem", command.Id));

        item.Update(
            brand: command.Brand,
            name: command.Name,
            lensType: command.LensType,
            material: (LensMaterial)command.Material,
            availableCoatings: (LensCoating)command.AvailableCoatings,
            sellingPrice: command.SellingPrice,
            costPrice: command.CostPrice,
            supplierId: command.PreferredSupplierId);

        // Handle activation state
        if (command.IsActive && !item.IsActive)
            item.Activate();
        else if (!command.IsActive && item.IsActive)
            item.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
