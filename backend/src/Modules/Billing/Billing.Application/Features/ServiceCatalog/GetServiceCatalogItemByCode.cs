using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Domain;

namespace Billing.Application.Features.ServiceCatalog;

/// <summary>
/// Query to retrieve an active service catalog item by its code.
/// </summary>
public sealed record GetServiceCatalogItemByCodeQuery(string Code);

/// <summary>
/// Wolverine static handler for looking up a service catalog item by code.
/// Returns null if not found or inactive.
/// </summary>
public static class GetServiceCatalogItemByCodeHandler
{
    public static async Task<Result<ServiceCatalogItemDto?>> Handle(
        GetServiceCatalogItemByCodeQuery query,
        IServiceCatalogRepository repository,
        CancellationToken ct)
    {
        var item = await repository.GetActiveByCodeAsync(query.Code, ct);

        if (item is null)
            return Result.Success<ServiceCatalogItemDto?>(null);

        return Result.Success<ServiceCatalogItemDto?>(
            CreateServiceCatalogItemHandler.MapToDto(item));
    }
}
