using FluentValidation;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Entities;
using Treatment.Domain.Enums;

namespace Treatment.Application.Features;

/// <summary>
/// Command to create a treatment package for a patient from a protocol template (TRT-01).
/// Nullable fields default to the template values when not provided.
/// </summary>
public sealed record CreateTreatmentPackageCommand(
    Guid ProtocolTemplateId,
    Guid PatientId,
    string PatientName,
    int? TotalSessions,
    int? PricingMode,
    decimal? PackagePrice,
    decimal? SessionPrice,
    int? MinIntervalDays,
    string? ParametersJson,
    Guid? VisitId);

/// <summary>
/// Validator for <see cref="CreateTreatmentPackageCommand"/>.
/// </summary>
public class CreateTreatmentPackageCommandValidator : AbstractValidator<CreateTreatmentPackageCommand>
{
    public CreateTreatmentPackageCommandValidator()
    {
        RuleFor(x => x.ProtocolTemplateId).NotEmpty().WithMessage("Protocol Template ID is required.");
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient ID is required.");
        RuleFor(x => x.PatientName).NotEmpty().WithMessage("Patient name is required.")
            .MaximumLength(200).WithMessage("Patient name must not exceed 200 characters.");
        RuleFor(x => x.TotalSessions)
            .InclusiveBetween(1, 6)
            .When(x => x.TotalSessions.HasValue)
            .WithMessage("Total sessions must be between 1 and 6.");
    }
}

/// <summary>
/// Wolverine static handler for creating a treatment package from a protocol template.
/// Loads template for defaults, applies optional overrides, creates TreatmentPackage via factory, persists.
/// </summary>
public static class CreateTreatmentPackageHandler
{
    public static async Task<Result<TreatmentPackageDto>> Handle(
        CreateTreatmentPackageCommand command,
        ITreatmentProtocolRepository protocolRepository,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateTreatmentPackageCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<TreatmentPackageDto>(Error.ValidationWithDetails(errors));
        }

        var template = await protocolRepository.GetByIdAsync(command.ProtocolTemplateId, ct);
        if (template is null)
            return Result.Failure<TreatmentPackageDto>(
                Error.NotFound("TreatmentProtocol", command.ProtocolTemplateId));

        // Use overrides if provided, otherwise fall back to template defaults
        var totalSessions = command.TotalSessions ?? template.DefaultSessionCount;
        var pricingMode = command.PricingMode.HasValue
            ? (PricingMode)command.PricingMode.Value
            : template.PricingMode;
        var packagePrice = command.PackagePrice ?? template.DefaultPackagePrice;
        var sessionPrice = command.SessionPrice ?? template.DefaultSessionPrice;
        var minIntervalDays = command.MinIntervalDays ?? template.MinIntervalDays;
        var parametersJson = command.ParametersJson ?? template.DefaultParametersJson ?? "{}";

        var package = TreatmentPackage.Create(
            protocolTemplateId: template.Id,
            patientId: command.PatientId,
            patientName: command.PatientName,
            treatmentType: template.TreatmentType,
            totalSessions: totalSessions,
            pricingMode: pricingMode,
            packagePrice: packagePrice,
            sessionPrice: sessionPrice,
            minIntervalDays: minIntervalDays,
            parametersJson: parametersJson,
            visitId: command.VisitId,
            createdById: currentUser.UserId,
            branchId: new BranchId(currentUser.BranchId));

        packageRepository.Add(package);
        await unitOfWork.SaveChangesAsync(ct);

        return MapToDto(package, template.Name);
    }

    /// <summary>
    /// Maps a TreatmentPackage entity to TreatmentPackageDto.
    /// Shared by all package handlers for consistent mapping.
    /// </summary>
    internal static TreatmentPackageDto MapToDto(TreatmentPackage package, string protocolTemplateName)
    {
        DateTime? lastSessionDate = null;
        DateTime? nextDueDate = null;

        var completedSessions = package.Sessions
            .Where(s => s.Status == SessionStatus.Completed)
            .OrderByDescending(s => s.CompletedAt)
            .ToList();

        if (completedSessions.Count > 0)
        {
            lastSessionDate = completedSessions[0].CompletedAt;
            if (package.SessionsRemaining > 0 && lastSessionDate.HasValue)
            {
                nextDueDate = lastSessionDate.Value.AddDays(package.MinIntervalDays);
            }
        }

        return new TreatmentPackageDto(
            Id: package.Id,
            ProtocolTemplateId: package.ProtocolTemplateId,
            ProtocolTemplateName: protocolTemplateName,
            PatientId: package.PatientId,
            PatientName: package.PatientName,
            TreatmentType: package.TreatmentType.ToString(),
            Status: package.Status.ToString(),
            TotalSessions: package.TotalSessions,
            SessionsCompleted: package.SessionsCompleted,
            SessionsRemaining: package.SessionsRemaining,
            PricingMode: package.PricingMode.ToString(),
            PackagePrice: package.PackagePrice,
            SessionPrice: package.SessionPrice,
            MinIntervalDays: package.MinIntervalDays,
            ParametersJson: package.ParametersJson,
            VisitId: package.VisitId,
            CreatedAt: package.CreatedAt,
            LastSessionDate: lastSessionDate,
            NextDueDate: nextDueDate,
            Sessions: package.Sessions.Select(s => new TreatmentSessionDto(
                Id: s.Id,
                SessionNumber: s.SessionNumber,
                Status: s.Status.ToString(),
                ParametersJson: s.ParametersJson,
                OsdiScore: s.OsdiScore,
                OsdiSeverity: s.OsdiSeverity,
                ClinicalNotes: s.ClinicalNotes,
                PerformedById: s.PerformedById,
                PerformedByName: null,
                VisitId: s.VisitId,
                ScheduledAt: s.ScheduledAt,
                CompletedAt: s.CompletedAt,
                CreatedAt: s.CreatedAt,
                IntervalOverrideReason: s.IntervalOverrideReason,
                Consumables: s.Consumables.Select(c => new SessionConsumableDto(
                    Id: c.Id,
                    ConsumableItemId: c.ConsumableItemId,
                    ConsumableName: c.ConsumableName,
                    Quantity: c.Quantity)).ToList())).ToList(),
            CancellationRequest: package.CancellationRequest is not null
                ? new CancellationRequestDto(
                    Id: package.CancellationRequest.Id,
                    RequestedById: package.CancellationRequest.RequestedById,
                    RequestedByName: "",
                    RequestedAt: package.CancellationRequest.RequestedAt,
                    Reason: package.CancellationRequest.Reason,
                    DeductionPercent: package.CancellationRequest.DeductionPercent,
                    RefundAmount: package.CancellationRequest.RefundAmount,
                    Status: package.CancellationRequest.Status.ToString(),
                    ApprovedById: package.CancellationRequest.ProcessedById,
                    ApprovedByName: null,
                    ApprovedAt: package.CancellationRequest.ProcessedAt,
                    RejectionReason: package.CancellationRequest.ProcessingNote)
                : null);
    }
}
