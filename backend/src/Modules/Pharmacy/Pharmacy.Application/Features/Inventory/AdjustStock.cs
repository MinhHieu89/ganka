using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Application.Features.Inventory;

/// <summary>
/// Command to manually adjust the stock quantity of a drug batch.
/// PHR-01: Supports manual stock corrections, write-offs, damage recording, and inventory count corrections.
/// Creates an audit StockAdjustment record for full traceability.
/// </summary>
/// <param name="DrugBatchId">The drug batch to adjust.</param>
/// <param name="QuantityChange">Signed quantity change: positive adds stock, negative removes stock. Must not be zero.</param>
/// <param name="Reason">The reason category for this adjustment (used for reporting and audit).</param>
/// <param name="Notes">Optional detail notes about the adjustment.</param>
public sealed record AdjustStockCommand(
    Guid DrugBatchId,
    int QuantityChange,
    StockAdjustmentReason Reason,
    string? Notes);

/// <summary>
/// Validates the AdjustStockCommand.
/// </summary>
public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.DrugBatchId)
            .NotEmpty().WithMessage("Drug batch ID is required.");

        RuleFor(x => x.QuantityChange)
            .NotEqual(0).WithMessage("Quantity change must not be zero. Use a non-zero signed value.");
    }
}

/// <summary>
/// Wolverine static handler for manually adjusting drug batch stock.
///
/// Workflow:
/// 1. Validate command
/// 2. Retrieve the drug batch
/// 3. Apply quantity change via domain method (AddStock or Deduct)
/// 4. Create StockAdjustment record for audit trail
/// 5. Save via unit of work
/// </summary>
public static class AdjustStockHandler
{
    public static async Task<Result<Guid>> Handle(
        AdjustStockCommand command,
        IValidator<AdjustStockCommand> validator,
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

        // Step 2: Retrieve the drug batch
        var batch = await drugBatchRepository.GetByIdAsync(command.DrugBatchId, ct);
        if (batch is null)
            return Result.Failure<Guid>(Error.NotFound("DrugBatch", command.DrugBatchId));

        // Step 3: Apply quantity change via domain methods
        try
        {
            if (command.QuantityChange > 0)
                batch.AddStock(command.QuantityChange);
            else
                batch.Deduct(Math.Abs(command.QuantityChange)); // Deduct expects a positive quantity
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Guid>(Error.Custom(
                "StockAdjustment.InsufficientStock",
                ex.Message));
        }

        // Step 4: Create StockAdjustment record for audit trail
        var adjustment = StockAdjustment.Create(
            drugBatchId: command.DrugBatchId,
            consumableBatchId: null,
            quantityChange: command.QuantityChange,
            reason: command.Reason,
            notes: command.Notes,
            adjustedById: currentUser.UserId);

        drugBatchRepository.AddStockAdjustment(adjustment);

        // Step 5: Save
        await unitOfWork.SaveChangesAsync(ct);

        return adjustment.Id;
    }
}
