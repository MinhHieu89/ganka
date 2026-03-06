using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Services;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Application.Features.OtcSales;

/// <summary>
/// Input for a single drug line in an OTC sale.
/// </summary>
/// <param name="DrugCatalogItemId">The Pharmacy DrugCatalogItem being sold.</param>
/// <param name="DrugName">Drug name denormalized for audit records.</param>
/// <param name="Quantity">Quantity being sold (must be positive).</param>
/// <param name="UnitPrice">Selling price per unit at time of sale (price snapshot).</param>
public sealed record OtcSaleLineInput(
    Guid DrugCatalogItemId,
    string DrugName,
    int Quantity,
    decimal UnitPrice);

/// <summary>
/// Command to process a walk-in OTC sale.
/// PHR-06: Staff can process walk-in OTC sales without prescription.
/// Customer linkage is optional — anonymous sales are supported.
/// Payment collection is deferred to Phase 7 (Billing).
/// </summary>
public sealed record CreateOtcSaleCommand(
    Guid? PatientId,
    string? CustomerName,
    string? Notes,
    List<OtcSaleLineInput> Lines);

/// <summary>
/// Validates the CreateOtcSaleCommand.
/// </summary>
public class CreateOtcSaleCommandValidator : AbstractValidator<CreateOtcSaleCommand>
{
    public CreateOtcSaleCommandValidator()
    {
        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one sale line is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.DrugCatalogItemId)
                .NotEmpty().WithMessage("Drug catalog item ID is required.");

            line.RuleFor(l => l.DrugName)
                .NotEmpty().WithMessage("Drug name is required.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be positive.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be non-negative.");
        });
    }
}

/// <summary>
/// Wolverine static handler for processing a walk-in OTC sale.
///
/// Workflow:
/// 1. Validate command
/// 2. Create OtcSale aggregate
/// 3. For each line: FEFO batch allocation + stock deduction + add line to aggregate
/// 4. Save via repository + unit of work
///
/// Uses same FEFOAllocator as dispensing (no separate logic for OTC).
/// </summary>
public static class CreateOtcSaleHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateOtcSaleCommand command,
        IValidator<CreateOtcSaleCommand> validator,
        IOtcSaleRepository otcSaleRepository,
        IDrugBatchRepository drugBatchRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        // Step 1: Validate
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<Guid>(Error.ValidationWithDetails(errors));
        }

        // Step 2: Create OtcSale aggregate
        var sale = OtcSale.Create(
            patientId: command.PatientId,
            customerName: command.CustomerName,
            soldById: currentUser.UserId,
            notes: command.Notes,
            branchId: new BranchId(currentUser.BranchId));

        // Step 3: Process each line with FEFO batch allocation
        foreach (var lineInput in command.Lines)
        {
            // Automatic FEFO: get available batches and allocate earliest-expiry-first
            var availableBatches = await drugBatchRepository.GetAvailableBatchesFEFOAsync(
                lineInput.DrugCatalogItemId, ct);

            var allocations = FEFOAllocator.Allocate(availableBatches, lineInput.Quantity);

            if (allocations.Count == 0)
                return Result.Failure<Guid>(Error.Custom(
                    "OtcSaleLine.InsufficientStock",
                    $"Insufficient stock for '{lineInput.DrugName}'. Requested: {lineInput.Quantity} units. " +
                    "No available batches with sufficient stock found."));

            // Add the sale line to the aggregate
            var saleLine = sale.AddLine(
                drugCatalogItemId: lineInput.DrugCatalogItemId,
                drugName: lineInput.DrugName,
                quantity: lineInput.Quantity,
                unitPrice: lineInput.UnitPrice);

            // Deduct stock from batches and record batch deductions
            foreach (var allocation in allocations)
            {
                saleLine.AddBatchDeduction(allocation.BatchId, allocation.BatchNumber, allocation.Quantity);

                // Deduct stock from batch entity (domain invariant: no negative stock)
                var batch = await drugBatchRepository.GetByIdAsync(allocation.BatchId, ct);
                if (batch is not null)
                    batch.Deduct(allocation.Quantity);
            }
        }

        // Step 4: Save
        otcSaleRepository.Add(sale);
        await unitOfWork.SaveChangesAsync(ct);

        return sale.Id;
    }
}
