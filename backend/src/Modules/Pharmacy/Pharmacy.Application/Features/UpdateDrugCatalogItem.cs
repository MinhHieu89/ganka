using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Application.Features;

/// <summary>
/// Command to update an existing drug catalog item.
/// </summary>
public sealed record UpdateDrugCatalogItemCommand(
    Guid Id,
    string Name,
    string NameVi,
    string GenericName,
    int Form,
    string? Strength,
    int Route,
    string Unit,
    string? DefaultDosageTemplate);

/// <summary>
/// Validator for <see cref="UpdateDrugCatalogItemCommand"/>.
/// </summary>
public class UpdateDrugCatalogItemCommandValidator : AbstractValidator<UpdateDrugCatalogItemCommand>
{
    public UpdateDrugCatalogItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Drug catalog item ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Drug name is required.")
            .MaximumLength(200).WithMessage("Drug name must not exceed 200 characters.");
        RuleFor(x => x.NameVi).NotEmpty().WithMessage("Vietnamese drug name is required.")
            .MaximumLength(200).WithMessage("Vietnamese drug name must not exceed 200 characters.");
        RuleFor(x => x.GenericName).NotEmpty().WithMessage("Generic name is required.")
            .MaximumLength(200).WithMessage("Generic name must not exceed 200 characters.");
        RuleFor(x => x.Unit).NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");
        RuleFor(x => x.Form).Must(f => Enum.IsDefined(typeof(DrugForm), f))
            .WithMessage("Form must be a valid drug form value.");
        RuleFor(x => x.Route).Must(r => Enum.IsDefined(typeof(DrugRoute), r))
            .WithMessage("Route must be a valid drug route value.");
    }
}

/// <summary>
/// Wolverine static handler for updating an existing drug catalog item.
/// Loads entity by ID, returns NotFound if missing, calls entity Update method.
/// </summary>
public static class UpdateDrugCatalogItemHandler
{
    public static async Task<Result> Handle(
        UpdateDrugCatalogItemCommand command,
        IDrugCatalogItemRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateDrugCatalogItemCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure(Error.ValidationWithDetails(errors));
        }

        var item = await repository.GetByIdAsync(command.Id, ct);
        if (item is null)
            return Result.Failure(Error.NotFound("DrugCatalogItem", command.Id));

        item.Update(
            command.Name,
            command.NameVi,
            command.GenericName,
            (DrugForm)command.Form,
            command.Strength,
            (DrugRoute)command.Route,
            command.Unit,
            command.DefaultDosageTemplate);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
