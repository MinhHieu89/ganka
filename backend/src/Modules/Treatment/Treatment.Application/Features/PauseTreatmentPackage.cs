using FluentValidation;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Entities;

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
/// </summary>
public static class PauseTreatmentPackageHandler
{
    public static async Task<Result<TreatmentPackageDto>> Handle(
        PauseTreatmentPackageCommand command,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IValidator<PauseTreatmentPackageCommand> validator,
        CancellationToken cancellationToken)
    {
        // Stub -- will be implemented in GREEN phase
        throw new NotImplementedException();
    }
}
