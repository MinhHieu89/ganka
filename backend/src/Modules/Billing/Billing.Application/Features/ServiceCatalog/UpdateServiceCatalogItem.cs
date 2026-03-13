using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using FluentValidation;
using Shared.Domain;

namespace Billing.Application.Features.ServiceCatalog;

/// <summary>
/// Command to update an existing service catalog item.
/// </summary>
public sealed record UpdateServiceCatalogItemCommand(
    Guid Id,
    string Name,
    string NameVi,
    decimal Price,
    bool IsActive,
    string? Description);

/// <summary>
/// Validator for <see cref="UpdateServiceCatalogItemCommand"/>.
/// </summary>
public class UpdateServiceCatalogItemValidator : AbstractValidator<UpdateServiceCatalogItemCommand>
{
    public UpdateServiceCatalogItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Service catalog item ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(200).WithMessage("Service name must not exceed 200 characters.");

        RuleFor(x => x.NameVi)
            .NotEmpty().WithMessage("Vietnamese name is required.")
            .MaximumLength(200).WithMessage("Vietnamese name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}

/// <summary>
/// Wolverine static handler for updating a service catalog item.
/// Loads by ID, updates fields, and optionally activates/deactivates.
/// Price changes are automatically captured by the audit interceptor (FIN-09).
/// </summary>
public static class UpdateServiceCatalogItemHandler
{
    public static async Task<Result<ServiceCatalogItemDto>> Handle(
        UpdateServiceCatalogItemCommand command,
        IServiceCatalogRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateServiceCatalogItemCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<ServiceCatalogItemDto>(Error.ValidationWithDetails(errors));
        }

        var item = await repository.GetByIdAsync(command.Id, ct);
        if (item is null)
        {
            return Result.Failure<ServiceCatalogItemDto>(
                Error.NotFound("ServiceCatalogItem", command.Id));
        }

        item.Update(command.Name, command.NameVi, command.Price, command.Description);

        // Handle activation/deactivation
        if (command.IsActive && !item.IsActive)
            item.Activate();
        else if (!command.IsActive && item.IsActive)
            item.Deactivate();

        repository.Update(item);
        await unitOfWork.SaveChangesAsync(ct);

        return CreateServiceCatalogItemHandler.MapToDto(item);
    }
}
