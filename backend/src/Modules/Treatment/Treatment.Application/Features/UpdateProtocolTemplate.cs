using FluentValidation;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Enums;

namespace Treatment.Application.Features;

/// <summary>
/// Command to update an existing treatment protocol template.
/// </summary>
public sealed record UpdateProtocolTemplateCommand(
    Guid Id,
    string Name,
    int TreatmentType,
    int DefaultSessionCount,
    int PricingMode,
    decimal DefaultPackagePrice,
    decimal DefaultSessionPrice,
    int MinIntervalDays,
    int MaxIntervalDays,
    string? DefaultParametersJson,
    decimal CancellationDeductionPercent,
    string? Description);

/// <summary>
/// Validator for <see cref="UpdateProtocolTemplateCommand"/>.
/// Same validation rules as Create, plus Id must be non-empty.
/// </summary>
public class UpdateProtocolTemplateCommandValidator : AbstractValidator<UpdateProtocolTemplateCommand>
{
    public UpdateProtocolTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Protocol template ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.DefaultSessionCount)
            .InclusiveBetween(1, 6).WithMessage("Default session count must be between 1 and 6.");

        RuleFor(x => x.CancellationDeductionPercent)
            .InclusiveBetween(10, 20).WithMessage("Cancellation deduction percent must be between 10 and 20.");

        RuleFor(x => x.DefaultPackagePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Default package price must be non-negative.");

        RuleFor(x => x.DefaultSessionPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Default session price must be non-negative.");

        RuleFor(x => x.TreatmentType)
            .Must(t => Enum.IsDefined(typeof(TreatmentType), t))
            .WithMessage("Treatment type must be a valid value.");

        RuleFor(x => x.PricingMode)
            .Must(p => Enum.IsDefined(typeof(PricingMode), p))
            .WithMessage("Pricing mode must be a valid value.");

        RuleFor(x => x.MinIntervalDays)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum interval days must be non-negative.");

        RuleFor(x => x.MaxIntervalDays)
            .GreaterThanOrEqualTo(x => x.MinIntervalDays)
            .WithMessage("Maximum interval days must be greater than or equal to minimum interval days.");
    }
}

/// <summary>
/// Wolverine static handler for updating an existing treatment protocol template.
/// Validates input, loads entity by ID, applies updates, persists, maps to DTO.
/// </summary>
public static class UpdateProtocolTemplateHandler
{
    public static async Task<Result<TreatmentProtocolDto>> Handle(
        UpdateProtocolTemplateCommand command,
        ITreatmentProtocolRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateProtocolTemplateCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<TreatmentProtocolDto>(Error.ValidationWithDetails(errors));
        }

        var protocol = await repository.GetByIdAsync(command.Id, ct);
        if (protocol is null)
            return Result.Failure<TreatmentProtocolDto>(Error.NotFound("TreatmentProtocol", command.Id));

        protocol.Update(
            command.Name,
            (TreatmentType)command.TreatmentType,
            command.DefaultSessionCount,
            (PricingMode)command.PricingMode,
            command.DefaultPackagePrice,
            command.DefaultSessionPrice,
            command.MinIntervalDays,
            command.MaxIntervalDays,
            command.DefaultParametersJson,
            command.CancellationDeductionPercent,
            command.Description);

        await unitOfWork.SaveChangesAsync(ct);

        return CreateProtocolTemplateHandler.MapToDto(protocol);
    }
}
