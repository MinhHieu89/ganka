using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using FluentValidation;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="AddDrugPrescriptionCommand"/>.
/// </summary>
public class AddDrugPrescriptionCommandValidator : AbstractValidator<AddDrugPrescriptionCommand>
{
    public AddDrugPrescriptionCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one prescription item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.DrugName).NotEmpty().WithMessage("Drug name is required.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            item.RuleFor(i => i.Unit).NotEmpty().WithMessage("Unit is required.");
        });
    }
}

/// <summary>
/// Wolverine handler for adding a drug prescription with items to a visit.
/// Creates DrugPrescription via factory, adds items (catalog or off-catalog),
/// calls visit.AddDrugPrescription(), and saves.
/// </summary>
public static class AddDrugPrescriptionHandler
{
    public static async Task<Result<Guid>> Handle(
        AddDrugPrescriptionCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<AddDrugPrescriptionCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<Guid>.Failure(Error.ValidationWithDetails(errors));
        }

        var visit = await visitRepository.GetByIdWithDetailsAsync(command.VisitId, ct);
        if (visit is null)
            return Result<Guid>.Failure(Error.NotFound("Visit", command.VisitId));

        var prescription = DrugPrescription.Create(visit.Id, command.Notes);
        prescription.GeneratePrescriptionCode();

        // Add items -- catalog-linked or off-catalog
        for (var i = 0; i < command.Items.Count; i++)
        {
            var input = command.Items[i];
            PrescriptionItem item;

            if (input.DrugCatalogItemId.HasValue)
            {
                item = PrescriptionItem.CreateFromCatalog(
                    prescription.Id,
                    input.DrugCatalogItemId.Value,
                    input.DrugName,
                    input.GenericName,
                    input.Strength,
                    input.Form,
                    input.Route,
                    input.Dosage,
                    input.DosageOverride,
                    input.Quantity,
                    input.Unit,
                    input.Frequency,
                    input.DurationDays,
                    input.HasAllergyWarning,
                    i);
            }
            else
            {
                item = PrescriptionItem.CreateOffCatalog(
                    prescription.Id,
                    input.DrugName,
                    input.GenericName,
                    input.Strength,
                    input.Form,
                    input.Route,
                    input.Dosage,
                    input.DosageOverride,
                    input.Quantity,
                    input.Unit,
                    input.Frequency,
                    input.DurationDays,
                    input.HasAllergyWarning,
                    i);
            }

            prescription.AddItem(item);
            visitRepository.AddPrescriptionItem(item);
        }

        try
        {
            visit.AddDrugPrescription(prescription);
            visitRepository.AddDrugPrescription(prescription);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return prescription.Id;
    }
}
