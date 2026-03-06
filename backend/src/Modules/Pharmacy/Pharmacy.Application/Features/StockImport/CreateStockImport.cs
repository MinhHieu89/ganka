using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Application.Features.StockImport;

/// <summary>
/// Input data for a single drug line in a stock import.
/// One line per drug batch being imported: captures drug identity, batch details, quantity, and cost.
/// </summary>
public sealed record StockImportLineInput(
    Guid DrugCatalogItemId,
    string DrugName,
    string BatchNumber,
    DateOnly ExpiryDate,
    int Quantity,
    decimal PurchasePrice);

/// <summary>
/// Command to create a stock import from a physical supplier invoice.
/// PHR-02: Supplier invoice import for daily stock restocking.
/// </summary>
public sealed record CreateStockImportCommand(
    Guid SupplierId,
    string? InvoiceNumber,
    string? Notes,
    List<StockImportLineInput> Lines);

/// <summary>
/// Validates the CreateStockImportCommand.
/// Ensures supplier is provided, at least one line exists, and each line has valid data.
/// </summary>
public class CreateStockImportCommandValidator : AbstractValidator<CreateStockImportCommand>
{
    public CreateStockImportCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier is required.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one import line is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.DrugCatalogItemId)
                .NotEmpty().WithMessage("Drug catalog item is required.");

            line.RuleFor(l => l.BatchNumber)
                .NotEmpty().WithMessage("Batch number is required.")
                .MaximumLength(50).WithMessage("Batch number must not exceed 50 characters.");

            line.RuleFor(l => l.ExpiryDate)
                .Must(d => d > DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Expiry date must be in the future.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            line.RuleFor(l => l.PurchasePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Purchase price cannot be negative.");
        });
    }
}

/// <summary>
/// Wolverine static handler for creating a stock import from a supplier invoice.
/// Creates a StockImport aggregate, one DrugBatch per line, all in a single transaction.
/// </summary>
public static class CreateStockImportHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateStockImportCommand command,
        IStockImportRepository stockImportRepository,
        IDrugBatchRepository drugBatchRepository,
        ISupplierRepository supplierRepository,
        IDrugCatalogItemRepository drugCatalogItemRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateStockImportCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        // Validate the command
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<Guid>(Error.ValidationWithDetails(errors));
        }

        // Verify supplier exists
        var supplier = await supplierRepository.GetByIdAsync(command.SupplierId, ct);
        if (supplier is null)
            return Result.Failure<Guid>(Error.NotFound("Supplier", command.SupplierId));

        // Create StockImport aggregate
        var stockImport = Domain.Entities.StockImport.Create(
            supplierId: command.SupplierId,
            supplierName: supplier.Name,
            importSource: ImportSource.SupplierInvoice,
            invoiceNumber: command.InvoiceNumber,
            importedById: currentUser.UserId,
            notes: command.Notes,
            branchId: new BranchId(currentUser.BranchId));

        // Process each line: verify drug exists, add import line, create drug batch
        foreach (var lineInput in command.Lines)
        {
            var drug = await drugCatalogItemRepository.GetByIdAsync(lineInput.DrugCatalogItemId, ct);
            if (drug is null)
                return Result.Failure<Guid>(Error.NotFound("DrugCatalogItem", lineInput.DrugCatalogItemId));

            // Add line to stock import aggregate
            stockImport.AddLine(
                drugCatalogItemId: lineInput.DrugCatalogItemId,
                drugName: lineInput.DrugName,
                batchNumber: lineInput.BatchNumber,
                expiryDate: lineInput.ExpiryDate,
                quantity: lineInput.Quantity,
                purchasePrice: lineInput.PurchasePrice);

            // Create a DrugBatch for this import line
            var batch = DrugBatch.Create(
                drugCatalogItemId: lineInput.DrugCatalogItemId,
                supplierId: command.SupplierId,
                batchNumber: lineInput.BatchNumber,
                expiryDate: lineInput.ExpiryDate,
                quantity: lineInput.Quantity,
                purchasePrice: lineInput.PurchasePrice,
                stockImportId: stockImport.Id);

            drugBatchRepository.Add(batch);
        }

        stockImportRepository.Add(stockImport);
        await unitOfWork.SaveChangesAsync(ct);

        return stockImport.Id;
    }
}
