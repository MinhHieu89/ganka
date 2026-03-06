using FluentValidation;
using Pharmacy.Application.Interfaces;
using Shared.Domain;

namespace Pharmacy.Application.Features.DrugCatalog;

/// <summary>
/// Command to update the pricing and minimum stock level of a drug catalog item.
/// Pricing is managed separately from general catalog attributes to allow
/// pharmacists or managers to update prices independently of catalog admins.
/// PHR-01: Supports configurable minimum stock thresholds per drug.
/// </summary>
/// <param name="DrugCatalogItemId">The ID of the drug catalog item to update pricing for.</param>
/// <param name="SellingPrice">Optional selling price. Null means no selling price is set.</param>
/// <param name="MinStockLevel">Minimum stock level threshold for low-stock alert. 0 means no alert threshold.</param>
public sealed record UpdateDrugCatalogPricingCommand(
    Guid DrugCatalogItemId,
    decimal? SellingPrice,
    int MinStockLevel);

/// <summary>
/// Validator for <see cref="UpdateDrugCatalogPricingCommand"/>.
/// </summary>
public class UpdateDrugCatalogPricingCommandValidator : AbstractValidator<UpdateDrugCatalogPricingCommand>
{
    public UpdateDrugCatalogPricingCommandValidator()
    {
        RuleFor(x => x.DrugCatalogItemId)
            .NotEmpty().WithMessage("Drug catalog item ID is required.");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price must be greater than or equal to zero.")
            .When(x => x.SellingPrice.HasValue);

        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be greater than or equal to zero.");
    }
}

/// <summary>
/// Wolverine static handler for updating drug catalog item pricing.
/// Loads entity by ID, calls domain UpdatePricing method, persists via unit of work.
/// This handler is separate from UpdateDrugCatalogItemHandler because pricing
/// is managed independently by different roles (pharmacist/manager vs catalog admin).
/// </summary>
public static class UpdateDrugCatalogPricingHandler
{
    public static async Task<Result> Handle(
        UpdateDrugCatalogPricingCommand command,
        IDrugCatalogItemRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateDrugCatalogPricingCommand> validator,
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

        var item = await repository.GetByIdAsync(command.DrugCatalogItemId, ct);
        if (item is null)
            return Result.Failure(Error.NotFound("DrugCatalogItem", command.DrugCatalogItemId));

        item.UpdatePricing(command.SellingPrice, command.MinStockLevel);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
