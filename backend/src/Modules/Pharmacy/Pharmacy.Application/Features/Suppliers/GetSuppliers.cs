using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Application.Features.Suppliers;

/// <summary>
/// Query to retrieve all suppliers (both active and inactive).
/// </summary>
public sealed record GetSuppliersQuery;

/// <summary>
/// Wolverine static handler for retrieving all suppliers.
/// Returns all suppliers as DTOs for supplier management page (shows both active and inactive).
/// </summary>
public static class GetSuppliersHandler
{
    public static async Task<List<SupplierDto>> Handle(
        GetSuppliersQuery query,
        ISupplierRepository repository,
        CancellationToken ct)
    {
        var suppliers = await repository.GetAllAsync(ct);

        return suppliers.Select(s => new SupplierDto(
            s.Id,
            s.Name,
            s.ContactInfo,
            s.Phone,
            s.Email,
            s.IsActive)).ToList();
    }
}
