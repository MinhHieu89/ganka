using FluentValidation;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Entities;

namespace Treatment.Application.Features;

/// <summary>
/// Command to modify a treatment package mid-course (TRT-07).
/// Creates a version snapshot before applying changes.
/// </summary>
public sealed record ModifyTreatmentPackageCommand(
    Guid PackageId,
    int? TotalSessions,
    string? ParametersJson,
    int? MinIntervalDays,
    string Reason);

/// <summary>
/// Validator for <see cref="ModifyTreatmentPackageCommand"/>.
/// </summary>
public class ModifyTreatmentPackageCommandValidator : AbstractValidator<ModifyTreatmentPackageCommand>
{
    public ModifyTreatmentPackageCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Modification reason is required.");
        RuleFor(x => x.TotalSessions)
            .InclusiveBetween(1, 6)
            .When(x => x.TotalSessions.HasValue)
            .WithMessage("Total sessions must be between 1 and 6.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="ModifyTreatmentPackageCommand"/>.
/// </summary>
public static class ModifyTreatmentPackageHandler
{
    public static async Task<Result<TreatmentPackageDto>> Handle(
        ModifyTreatmentPackageCommand command,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IValidator<ModifyTreatmentPackageCommand> validator,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        // Stub -- will be implemented in GREEN phase
        throw new NotImplementedException();
    }
}
