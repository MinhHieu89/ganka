using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISupplierRepository"/>.
/// Provides CRUD operations for Supplier entities and SupplierDrugPrice queries.
/// </summary>
public sealed class SupplierRepository(PharmacyDbContext context) : ISupplierRepository
{
    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<List<Supplier>> GetAllActiveAsync(CancellationToken ct)
    {
        return await context.Suppliers
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<List<SupplierDrugPriceDto>> GetSupplierDrugPricesAsync(Guid supplierId, CancellationToken ct)
    {
        return await context.SupplierDrugPrices
            .AsNoTracking()
            .Where(p => p.SupplierId == supplierId)
            .Join(
                context.DrugCatalogItems,
                p => p.DrugCatalogItemId,
                d => d.Id,
                (p, d) => new SupplierDrugPriceDto(
                    p.Id,
                    p.SupplierId,
                    string.Empty, // SupplierName not needed here — caller already has supplier context
                    p.DrugCatalogItemId,
                    d.Name,
                    p.DefaultPurchasePrice))
            .OrderBy(dto => dto.DrugName)
            .ToListAsync(ct);
    }

    public async Task<decimal?> GetDefaultPriceAsync(Guid supplierId, Guid drugCatalogItemId, CancellationToken ct)
    {
        var price = await context.SupplierDrugPrices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.SupplierId == supplierId && p.DrugCatalogItemId == drugCatalogItemId,
                ct);

        return price?.DefaultPurchasePrice;
    }

    public void Add(Supplier supplier)
    {
        context.Suppliers.Add(supplier);
    }

    public void AddSupplierDrugPrice(SupplierDrugPrice price)
    {
        context.SupplierDrugPrices.Add(price);
    }

    public void UpdateSupplierDrugPrice(SupplierDrugPrice price)
    {
        context.SupplierDrugPrices.Update(price);
    }
}
