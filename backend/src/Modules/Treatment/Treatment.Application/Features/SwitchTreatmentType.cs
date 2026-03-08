using FluentValidation;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Entities;

namespace Treatment.Application.Features;

/// <summary>
/// Command to switch a patient from one treatment type to another mid-course (TRT-08).
/// Marks the old package as Switched and creates a new one from the new template
/// with remaining sessions.
/// </summary>
public sealed record SwitchTreatmentTypeCommand(
    Guid PackageId,
    Guid NewProtocolTemplateId,
    string Reason);

/// <summary>
/// Validator for <see cref="SwitchTreatmentTypeCommand"/>.
/// </summary>
public class SwitchTreatmentTypeCommandValidator : AbstractValidator<SwitchTreatmentTypeCommand>
{
    public SwitchTreatmentTypeCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.NewProtocolTemplateId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Switch reason is required.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="SwitchTreatmentTypeCommand"/>.
/// </summary>
public static class SwitchTreatmentTypeHandler
{
    public static async Task<Result<TreatmentPackageDto>> Handle(
        SwitchTreatmentTypeCommand command,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        IUnitOfWork unitOfWork,
        IValidator<SwitchTreatmentTypeCommand> validator,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        // Stub -- will be implemented in GREEN phase
        throw new NotImplementedException();
    }
}
