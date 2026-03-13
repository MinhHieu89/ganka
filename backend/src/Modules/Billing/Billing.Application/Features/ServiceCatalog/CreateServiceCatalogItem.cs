using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features.ServiceCatalog;

/// <summary>
/// Command to create a new service catalog item.
/// </summary>
public sealed record CreateServiceCatalogItemCommand(
    string Code,
    string Name,
    string NameVi,
    decimal Price,
    string? Description);

/// <summary>
/// Validator for <see cref="CreateServiceCatalogItemCommand"/>.
/// </summary>
public class CreateServiceCatalogItemValidator : AbstractValidator<CreateServiceCatalogItemCommand>
{
    public CreateServiceCatalogItemValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Service code is required.")
            .MaximumLength(50).WithMessage("Service code must not exceed 50 characters.");

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
/// Wolverine static handler for creating a new service catalog item.
/// Checks for duplicate code before creating.
/// </summary>
public static class CreateServiceCatalogItemHandler
{
    public static async Task<Result<ServiceCatalogItemDto>> Handle(
        CreateServiceCatalogItemCommand command,
        IServiceCatalogRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateServiceCatalogItemCommand> validator,
        ICurrentUser currentUser,
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

        var normalizedCode = command.Code.ToUpperInvariant();

        // Check for duplicate code
        var existing = await repository.GetActiveByCodeAsync(normalizedCode, ct);
        if (existing is not null)
        {
            return Result.Failure<ServiceCatalogItemDto>(
                Error.Conflict($"A service catalog item with code '{normalizedCode}' already exists."));
        }

        var item = ServiceCatalogItem.Create(
            normalizedCode,
            command.Name,
            command.NameVi,
            command.Price,
            new BranchId(currentUser.BranchId),
            command.Description);

        repository.Add(item);
        await unitOfWork.SaveChangesAsync(ct);

        return MapToDto(item);
    }

    internal static ServiceCatalogItemDto MapToDto(ServiceCatalogItem item)
    {
        return new ServiceCatalogItemDto(
            Id: item.Id,
            Code: item.Code,
            Name: item.Name,
            NameVi: item.NameVi,
            Price: item.Price,
            IsActive: item.IsActive,
            Description: item.Description,
            CreatedAt: item.CreatedAt,
            UpdatedAt: item.UpdatedAt);
    }
}
