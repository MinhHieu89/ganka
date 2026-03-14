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
/// Implements close-and-create pattern: marks old package as Switched,
/// creates new package from new template with remaining session count.
/// Both operations saved in a single UnitOfWork transaction.
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
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<TreatmentPackageDto>.Failure(Error.ValidationWithDetails(errors));
        }

        // 1. Load existing package (check status is modifiable)
        var existingPackage = await packageRepository.GetByIdAsync(command.PackageId, cancellationToken);
        if (existingPackage is null)
            return Result<TreatmentPackageDto>.Failure(
                Error.NotFound("TreatmentPackage", command.PackageId));

        // 2. Load new protocol template BEFORE mutating the existing package
        var newTemplate = await protocolRepository.GetByIdAsync(command.NewProtocolTemplateId, cancellationToken);
        if (newTemplate is null)
            return Result<TreatmentPackageDto>.Failure(
                Error.NotFound("TreatmentProtocol", command.NewProtocolTemplateId));

        // 3. Mark existing package as switched (validates modifiable status)
        try
        {
            existingPackage.MarkAsSwitched();
        }
        catch (InvalidOperationException ex)
        {
            return Result<TreatmentPackageDto>.Failure(Error.Validation(ex.Message));
        }

        // 4. Calculate remaining sessions
        var remainingSessions = existingPackage.TotalSessions - existingPackage.SessionsCompleted;
        if (remainingSessions < 1) remainingSessions = 1; // Ensure at least 1 session

        // 5. Create new package from template with remaining sessions
        var newPackage = TreatmentPackage.Create(
            protocolTemplateId: newTemplate.Id,
            patientId: existingPackage.PatientId,
            patientName: existingPackage.PatientName,
            treatmentType: newTemplate.TreatmentType,
            totalSessions: remainingSessions,
            pricingMode: newTemplate.PricingMode,
            packagePrice: newTemplate.DefaultPackagePrice,
            sessionPrice: newTemplate.DefaultSessionPrice,
            minIntervalDays: newTemplate.MinIntervalDays,
            parametersJson: newTemplate.DefaultParametersJson ?? "{}",
            visitId: null,
            createdById: currentUser.UserId,
            branchId: new BranchId(currentUser.BranchId));

        // 6. Save both changes in single UnitOfWork transaction
        packageRepository.Add(newPackage);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return new package DTO
        return CreateTreatmentPackageHandler.MapToDto(newPackage, newTemplate.Name);
    }
}
