using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Domain;

namespace Billing.Application.Features.ServiceCatalog;

/// <summary>
/// Query to retrieve all service catalog items.
/// </summary>
public sealed record GetServiceCatalogItemsQuery(bool IncludeInactive = false);

/// <summary>
/// Wolverine static handler for listing service catalog items.
/// Returns active items by default; optionally includes inactive.
/// </summary>
public static class GetServiceCatalogItemsHandler
{
    public static async Task<Result<List<ServiceCatalogItemDto>>> Handle(
        GetServiceCatalogItemsQuery query,
        IServiceCatalogRepository repository,
        CancellationToken ct)
    {
        var items = await repository.GetAllAsync(query.IncludeInactive, ct);

        var dtos = items.Select(CreateServiceCatalogItemHandler.MapToDto).ToList();

        return dtos;
    }
}
