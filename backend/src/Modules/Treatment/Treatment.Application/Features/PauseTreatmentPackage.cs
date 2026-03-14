using FluentValidation;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;

namespace Treatment.Application.Features;

/// <summary>
/// Enum for pause/resume action selection.
/// </summary>
public enum PauseAction
{
    Pause = 0,
    Resume = 1
}

/// <summary>
/// Command to pause or resume a treatment package.
/// </summary>
public sealed record PauseTreatmentPackageCommand(
    Guid PackageId,
    PauseAction Action);

/// <summary>
/// Validator for <see cref="PauseTreatmentPackageCommand"/>.
/// </summary>
public class PauseTreatmentPackageCommandValidator : AbstractValidator<PauseTreatmentPackageCommand>
{
    public PauseTreatmentPackageCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.Action).IsInEnum();
    }
}

/// <summary>
/// Wolverine handler for <see cref="PauseTreatmentPackageCommand"/>.
/// Pauses an active package or resumes a paused package.
/// </summary>
public static class PauseTreatmentPackageHandler
{
    public static async Task<Result<TreatmentPackageDto>> Handle(
        PauseTreatmentPackageCommand command,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        IUnitOfWork unitOfWork,
        IValidator<PauseTreatmentPackageCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<TreatmentPackageDto>.Failure(Error.ValidationWithDetails(errors));
        }

        var package = await packageRepository.GetByIdAsync(command.PackageId, cancellationToken);
        if (package is null)
            return Result<TreatmentPackageDto>.Failure(
                Error.NotFound("TreatmentPackage", command.PackageId));

        try
        {
            if (command.Action == PauseAction.Pause)
                package.Pause();
            else
                package.Resume();
        }
        catch (InvalidOperationException ex)
        {
            return Result<TreatmentPackageDto>.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Load protocol template name for the DTO
        var protocol = await protocolRepository.GetByIdAsync(package.ProtocolTemplateId, cancellationToken);
        return CreateTreatmentPackageHandler.MapToDto(package, protocol?.Name ?? "");
    }
}
