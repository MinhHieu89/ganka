using FluentValidation;
using Optical.Application.Interfaces;
using Optical.Domain.Enums;
using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Command to update an existing frame's details.
/// </summary>
public sealed record UpdateFrameCommand(
    Guid Id,
    string Brand,
    string Model,
    string Color,
    int LensWidth,
    int BridgeWidth,
    int TempleLength,
    int Material,
    int FrameType,
    int Gender,
    decimal SellingPrice,
    decimal CostPrice,
    string? Barcode,
    int StockQuantity,
    int MinStockLevel,
    bool IsActive);

/// <summary>
/// Validator for <see cref="UpdateFrameCommand"/>.
/// Enforces optical-specific size ranges and barcode format.
/// </summary>
public class UpdateFrameCommandValidator : AbstractValidator<UpdateFrameCommand>
{
    public UpdateFrameCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Frame ID is required.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(100).WithMessage("Model must not exceed 100 characters.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required.")
            .MaximumLength(50).WithMessage("Color must not exceed 50 characters.");

        RuleFor(x => x.LensWidth)
            .InclusiveBetween(40, 65).WithMessage("Lens width must be between 40 and 65 mm.");

        RuleFor(x => x.BridgeWidth)
            .InclusiveBetween(12, 24).WithMessage("Bridge width must be between 12 and 24 mm.");

        RuleFor(x => x.TempleLength)
            .InclusiveBetween(120, 155).WithMessage("Temple length must be between 120 and 155 mm.");

        RuleFor(x => x.SellingPrice)
            .GreaterThan(0).WithMessage("Selling price must be greater than 0.");

        RuleFor(x => x.CostPrice)
            .GreaterThan(0).WithMessage("Cost price must be greater than 0.");

        RuleFor(x => x.Barcode)
            .Matches(@"^\d{13}$").WithMessage("Barcode must be exactly 13 digits.")
            .When(x => x.Barcode is not null);

        RuleFor(x => x.Material)
            .Must(m => Enum.IsDefined(typeof(FrameMaterial), m))
            .WithMessage("Material must be a valid frame material value.");

        RuleFor(x => x.FrameType)
            .Must(t => Enum.IsDefined(typeof(FrameType), t))
            .WithMessage("Frame type must be a valid frame type value.");

        RuleFor(x => x.Gender)
            .Must(g => Enum.IsDefined(typeof(FrameGender), g))
            .WithMessage("Gender must be a valid frame gender value.");
    }
}

/// <summary>
/// Wolverine static handler for updating an existing frame.
/// Loads entity by ID, returns NotFound if missing, validates, checks barcode uniqueness (excluding self), updates.
/// </summary>
public static class UpdateFrameHandler
{
    public static async Task<Result> Handle(
        UpdateFrameCommand command,
        IFrameRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateFrameCommand> validator,
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

        var frame = await repository.GetByIdAsync(command.Id, ct);
        if (frame is null)
            return Result.Failure(Error.NotFound("Frame", command.Id));

        if (command.Barcode is not null)
        {
            var isUnique = await repository.IsBarcodeUniqueAsync(command.Barcode, frame.Id, ct);
            if (!isUnique)
                return Result.Failure(Error.Conflict($"Barcode '{command.Barcode}' is already used by another frame."));
        }

        frame.Update(
            command.Brand,
            command.Model,
            command.Color,
            command.LensWidth,
            command.BridgeWidth,
            command.TempleLength,
            (FrameMaterial)command.Material,
            (FrameType)command.FrameType,
            (FrameGender)command.Gender,
            command.SellingPrice,
            command.CostPrice,
            command.Barcode);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
