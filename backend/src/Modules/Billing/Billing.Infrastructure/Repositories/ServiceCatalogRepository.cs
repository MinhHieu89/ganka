using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IServiceCatalogRepository"/>.
/// Provides CRUD operations for service catalog items.
/// </summary>
public sealed class ServiceCatalogRepository(BillingDbContext context) : IServiceCatalogRepository
{
    public async Task<ServiceCatalogItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.ServiceCatalogItems
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<ServiceCatalogItem?> GetActiveByCodeAsync(string code, CancellationToken ct)
    {
        return await context.ServiceCatalogItems
            .FirstOrDefaultAsync(s => s.Code == code && s.IsActive, ct);
    }

    public async Task<List<ServiceCatalogItem>> GetAllAsync(
        bool includeInactive = false, CancellationToken ct = default)
    {
        var query = context.ServiceCatalogItems.AsQueryable();

        if (!includeInactive)
            query = query.Where(s => s.IsActive);

        return await query.OrderBy(s => s.Code).ToListAsync(ct);
    }

    public void Add(ServiceCatalogItem item)
    {
        context.ServiceCatalogItems.Add(item);
    }

    public void Update(ServiceCatalogItem item)
    {
        context.ServiceCatalogItems.Update(item);
    }
}
