using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentValidation;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="AddVisitDiagnosisCommand"/>.
/// </summary>
public class AddVisitDiagnosisCommandValidator : AbstractValidator<AddVisitDiagnosisCommand>
{
    public AddVisitDiagnosisCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.Icd10Code).NotEmpty().WithMessage("ICD-10 code is required.");
        RuleFor(x => x.Laterality).Must(l => Enum.IsDefined(typeof(Laterality), l))
            .WithMessage("Laterality must be a valid value (0=OD, 1=OS, 2=OU).");
    }
}

/// <summary>
/// Wolverine handler for adding a diagnosis to a visit.
/// If laterality is OU, creates two records: one OD and one OS.
/// Sets first diagnosis as Primary, subsequent as Secondary.
/// </summary>
public static class AddVisitDiagnosisHandler
{
    public static async Task<Result> Handle(
        AddVisitDiagnosisCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<AddVisitDiagnosisCommand> validator,
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

        var visit = await visitRepository.GetByIdWithDetailsAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        var laterality = (Laterality)command.Laterality;
        var role = (DiagnosisRole)command.Role;

        // Auto-determine role: first diagnosis is Primary, rest Secondary
        if (!visit.Diagnoses.Any())
            role = DiagnosisRole.Primary;
        else if (role == DiagnosisRole.Primary && visit.Diagnoses.Any(d => d.Role == DiagnosisRole.Primary))
            role = DiagnosisRole.Secondary;

        try
        {
            if (laterality == Laterality.OU)
            {
                // Create two records: one for OD (.1 suffix) and one for OS (.2 suffix)
                var baseCode = command.Icd10Code;
                var odCode = baseCode.EndsWith(".") ? baseCode + "1" : baseCode + ".1";
                var osCode = baseCode.EndsWith(".") ? baseCode + "2" : baseCode + ".2";

                var odDiagnosis = VisitDiagnosis.Create(
                    visit.Id, odCode,
                    command.DescriptionEn + " (right eye)",
                    command.DescriptionVi + " (mat phai)",
                    Laterality.OD, role, command.SortOrder);

                var osDiagnosis = VisitDiagnosis.Create(
                    visit.Id, osCode,
                    command.DescriptionEn + " (left eye)",
                    command.DescriptionVi + " (mat trai)",
                    Laterality.OS,
                    role == DiagnosisRole.Primary ? DiagnosisRole.Secondary : role,
                    command.SortOrder + 1);

                visit.AddDiagnosis(odDiagnosis);
                visitRepository.AddDiagnosis(odDiagnosis);
                visit.AddDiagnosis(osDiagnosis);
                visitRepository.AddDiagnosis(osDiagnosis);
            }
            else
            {
                var diagnosis = VisitDiagnosis.Create(
                    visit.Id, command.Icd10Code,
                    command.DescriptionEn, command.DescriptionVi,
                    laterality, role, command.SortOrder);

                visit.AddDiagnosis(diagnosis);
                visitRepository.AddDiagnosis(diagnosis);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
