using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOtcSaleRepository"/>.
/// Provides paginated OTC sale listing with eager-loaded Lines.
/// </summary>
public sealed class OtcSaleRepository(PharmacyDbContext context) : IOtcSaleRepository
{
    public async Task<OtcSale?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.OtcSales
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<(List<OtcSaleDto> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = context.OtcSales
            .AsNoTracking()
            .OrderByDescending(s => s.SoldAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.Lines)
            .Select(s => new OtcSaleDto(
                s.Id,
                s.PatientId,
                s.CustomerName,
                s.SoldAt,
                s.Notes,
                s.Lines.Select(l => new OtcSaleLineDto(
                    l.Id,
                    l.DrugCatalogItemId,
                    l.DrugName,
                    l.Quantity,
                    l.UnitPrice)).ToList()))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public void Add(OtcSale sale)
    {
        context.OtcSales.Add(sale);
    }
}
