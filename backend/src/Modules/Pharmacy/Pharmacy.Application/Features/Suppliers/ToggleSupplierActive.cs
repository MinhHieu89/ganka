using Pharmacy.Application.Interfaces;
using Shared.Domain;

namespace Pharmacy.Application.Features.Suppliers;

/// <summary>
/// Command to toggle a supplier's active/inactive status.
/// Calls Activate() on inactive suppliers and Deactivate() on active ones.
/// </summary>
public sealed record ToggleSupplierActiveCommand(Guid Id);

/// <summary>
/// Wolverine static handler for toggling supplier active status.
/// No validator needed — only requires a valid supplier ID.
/// </summary>
public static class ToggleSupplierActiveHandler
{
    public static async Task<Result> Handle(
        ToggleSupplierActiveCommand command,
        ISupplierRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var supplier = await repository.GetByIdAsync(command.Id, ct);
        if (supplier is null)
            return Result.Failure(Error.NotFound("Supplier", command.Id));

        if (supplier.IsActive)
            supplier.Deactivate();
        else
            supplier.Activate();

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
