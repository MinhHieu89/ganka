using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Services;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Application.Features.Dispensing;

/// <summary>
/// Input for a single prescription drug line to be dispensed.
/// </summary>
/// <param name="PrescriptionItemId">The Clinical PrescriptionItem.Id this line corresponds to.</param>
/// <param name="DrugCatalogItemId">The Pharmacy DrugCatalogItem being dispensed. Null for off-catalog drugs.</param>
/// <param name="DrugName">Drug name (denormalized from prescription for audit).</param>
/// <param name="Quantity">Quantity to dispense as specified in the prescription.</param>
/// <param name="IsOffCatalog">When true, this is a manually typed drug with no catalog entry. No batch deduction.</param>
/// <param name="Skip">When true, the pharmacist intentionally skips this line (out of stock, patient already has, etc.).</param>
/// <param name="ManualBatches">Optional manual FEFO override. When null, automatic FEFO batch selection is used.</param>
public sealed record DispenseLineInput(
    Guid PrescriptionItemId,
    Guid? DrugCatalogItemId,
    string DrugName,
    int Quantity,
    bool IsOffCatalog,
    bool Skip,
    List<BatchOverride>? ManualBatches);

/// <summary>
/// Manual batch override for FEFO selection.
/// Allows the pharmacist to specify exactly which batches to use (e.g., specific batch from back of shelf).
/// </summary>
/// <param name="BatchId">The specific DrugBatch.Id to use.</param>
/// <param name="Quantity">How many units to take from this batch.</param>
public sealed record BatchOverride(Guid BatchId, int Quantity);

/// <summary>
/// Command to dispense drugs against a HIS prescription.
/// PHR-05: Pharmacist dispenses drugs against HIS prescription with auto stock deduction.
/// PHR-07: 7-day prescription validity enforced with warn-but-allow override.
/// </summary>
public sealed record DispenseDrugsCommand(
    Guid PrescriptionId,
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    DateTime PrescribedAt,
    string? OverrideReason,
    List<DispenseLineInput> Lines);

/// <summary>
/// Validates the DispenseDrugsCommand.
/// </summary>
public class DispenseDrugsCommandValidator : AbstractValidator<DispenseDrugsCommand>
{
    public DispenseDrugsCommandValidator()
    {
        RuleFor(x => x.PrescriptionId)
            .NotEmpty().WithMessage("Prescription ID is required.");

        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient name is required.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one dispensing line is required.");
    }
}

/// <summary>
/// Wolverine static handler for dispensing drugs against a HIS prescription.
///
/// Workflow:
/// 1. Validate command
/// 2. Check for duplicate dispensing (GetByPrescriptionIdAsync)
/// 3. Enforce 7-day validity window (unless OverrideReason is provided)
/// 4. Create DispensingRecord aggregate
/// 5. For each line:
///    - Skip: record line with Skipped status, no batch deduction
///    - IsOffCatalog: record line with Dispensed status, no batch deduction (manual drug)
///    - Catalog drug: FEFO auto-select or manual override, deduct stock, record BatchDeductions
/// 6. Save via repository + unit of work
/// </summary>
public static class DispenseDrugsHandler
{
    /// <summary>Number of days a prescription remains valid after prescribing (PHR-07).</summary>
    private const int PrescriptionValidityDays = 7;

    public static async Task<Result<Guid>> Handle(
        DispenseDrugsCommand command,
        IDispensingRepository dispensingRepository,
        IDrugBatchRepository drugBatchRepository,
        IUnitOfWork unitOfWork,
        IValidator<DispenseDrugsCommand> validator,
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

        // Step 2: Check for duplicate dispensing
        var existing = await dispensingRepository.GetByPrescriptionIdAsync(command.PrescriptionId, ct);
        if (existing is not null)
            return Result.Failure<Guid>(Error.Custom(
                "Prescription.AlreadyDispensed",
                $"Prescription {command.PrescriptionId} has already been dispensed. Dispensing record: {existing.Id}"));

        // Step 3: Enforce 7-day prescription validity
        var expiresAt = command.PrescribedAt.AddDays(PrescriptionValidityDays);
        if (DateTime.UtcNow > expiresAt && string.IsNullOrWhiteSpace(command.OverrideReason))
            return Result.Failure<Guid>(Error.Custom(
                "Prescription.Expired",
                $"Prescription was issued {(int)(DateTime.UtcNow - command.PrescribedAt).TotalDays} days ago " +
                $"and has exceeded the {PrescriptionValidityDays}-day validity window. " +
                "Provide an override reason to dispense this expired prescription (PHR-07)."));

        // Step 4: Create DispensingRecord aggregate
        var record = DispensingRecord.Create(
            prescriptionId: command.PrescriptionId,
            visitId: command.VisitId,
            patientId: command.PatientId,
            patientName: command.PatientName,
            dispensedById: currentUser.UserId,
            overrideReason: string.IsNullOrWhiteSpace(command.OverrideReason) ? null : command.OverrideReason,
            branchId: new BranchId(currentUser.BranchId));

        // Step 5: Process each line
        foreach (var line in command.Lines)
        {
            if (line.Skip)
            {
                // Pharmacist intentionally skips this line — no stock deduction
                record.AddLine(
                    prescriptionItemId: line.PrescriptionItemId,
                    drugCatalogItemId: line.DrugCatalogItemId ?? Guid.Empty,
                    drugName: line.DrugName,
                    quantity: line.Quantity,
                    status: DispensingStatus.Skipped);
                continue;
            }

            if (line.IsOffCatalog)
            {
                // Off-catalog drug: dispensed as recorded, but no batch tracking
                record.AddLine(
                    prescriptionItemId: line.PrescriptionItemId,
                    drugCatalogItemId: Guid.Empty,
                    drugName: line.DrugName,
                    quantity: line.Quantity,
                    status: DispensingStatus.Dispensed);
                continue;
            }

            // Catalog drug: perform FEFO batch allocation and stock deduction
            if (!line.DrugCatalogItemId.HasValue)
                return Result.Failure<Guid>(Error.Custom(
                    "DispensingLine.MissingCatalogItem",
                    $"Drug '{line.DrugName}' is marked as catalog drug but DrugCatalogItemId is missing."));

            List<BatchAllocation> allocations;

            if (line.ManualBatches is { Count: > 0 })
            {
                // Manual FEFO override: pharmacist specified which batches to use
                allocations = await AllocateManualBatchesAsync(
                    line.ManualBatches, drugBatchRepository, line.DrugName, line.Quantity, ct);
            }
            else
            {
                // Automatic FEFO: get available batches and allocate earliest-expiry-first
                var availableBatches = await drugBatchRepository.GetAvailableBatchesFEFOAsync(
                    line.DrugCatalogItemId.Value, ct);
                allocations = FEFOAllocator.Allocate(availableBatches, line.Quantity);
            }

            if (allocations.Count == 0)
                return Result.Failure<Guid>(Error.Custom(
                    "DispensingLine.InsufficientStock",
                    $"Insufficient stock for '{line.DrugName}'. Requested: {line.Quantity} units. " +
                    "No available batches with sufficient stock found."));

            // Add the dispensing line and batch deductions
            var dispensingLine = record.AddLine(
                prescriptionItemId: line.PrescriptionItemId,
                drugCatalogItemId: line.DrugCatalogItemId.Value,
                drugName: line.DrugName,
                quantity: line.Quantity,
                status: DispensingStatus.Dispensed);

            foreach (var allocation in allocations)
            {
                dispensingLine.AddBatchDeduction(allocation.BatchId, allocation.BatchNumber, allocation.Quantity);

                // Deduct stock from batch entity (domain invariant: no negative stock)
                var batch = await drugBatchRepository.GetByIdAsync(allocation.BatchId, ct);
                if (batch is not null)
                    batch.Deduct(allocation.Quantity);
            }
        }

        // Step 6: Save
        dispensingRepository.Add(record);
        await unitOfWork.SaveChangesAsync(ct);

        return record.Id;
    }

    /// <summary>
    /// Resolves manual batch override entries into BatchAllocation records.
    /// Validates that the total quantity matches the line quantity.
    /// </summary>
    private static async Task<List<BatchAllocation>> AllocateManualBatchesAsync(
        List<BatchOverride> manualBatches,
        IDrugBatchRepository batchRepository,
        string drugName,
        int requiredQuantity,
        CancellationToken ct)
    {
        var allocations = new List<BatchAllocation>();
        var totalAllocated = 0;

        foreach (var manual in manualBatches)
        {
            var batch = await batchRepository.GetByIdAsync(manual.BatchId, ct);
            if (batch is null || batch.CurrentQuantity < manual.Quantity)
                return []; // Insufficient stock in specified batch

            allocations.Add(new BatchAllocation(batch.Id, batch.BatchNumber, manual.Quantity, batch.ExpiryDate));
            totalAllocated += manual.Quantity;
        }

        if (totalAllocated < requiredQuantity)
            return []; // Manual allocation did not cover full required quantity

        return allocations;
    }
}
