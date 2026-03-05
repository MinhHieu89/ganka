using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Application.Features;

/// <summary>
/// Command to create a new drug catalog item.
/// </summary>
public sealed record CreateDrugCatalogItemCommand(
    string Name,
    string NameVi,
    string GenericName,
    int Form,
    string? Strength,
    int Route,
    string Unit,
    string? DefaultDosageTemplate);

/// <summary>
/// Validator for <see cref="CreateDrugCatalogItemCommand"/>.
/// </summary>
public class CreateDrugCatalogItemCommandValidator : AbstractValidator<CreateDrugCatalogItemCommand>
{
    public CreateDrugCatalogItemCommandValidator()
    {
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
/// Wolverine static handler for creating a new drug catalog item.
/// Validates input, creates entity via factory method, persists via repository.
/// </summary>
public static class CreateDrugCatalogItemHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateDrugCatalogItemCommand command,
        IDrugCatalogItemRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateDrugCatalogItemCommand> validator,
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

        var item = DrugCatalogItem.Create(
            command.Name,
            command.NameVi,
            command.GenericName,
            (DrugForm)command.Form,
            command.Strength,
            (DrugRoute)command.Route,
            command.Unit,
            command.DefaultDosageTemplate,
            new BranchId(currentUser.BranchId));

        repository.Add(item);
        await unitOfWork.SaveChangesAsync(ct);

        return item.Id;
    }
}
