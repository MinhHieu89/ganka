using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IStockImportRepository"/>.
/// Provides paginated listing and detail retrieval for stock import events.
/// </summary>
public sealed class StockImportRepository(PharmacyDbContext context) : IStockImportRepository
{
    public async Task<StockImport?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.StockImports
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<(List<StockImportDto> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = context.StockImports
            .AsNoTracking()
            .OrderByDescending(s => s.ImportedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.Lines)
            .Select(s => new StockImportDto(
                s.Id,
                s.SupplierId,
                s.SupplierName,
                (int)s.ImportSource,
                s.InvoiceNumber,
                s.ImportedAt,
                s.Notes,
                s.Lines.Select(l => new StockImportLineDto(
                    l.Id,
                    l.DrugCatalogItemId,
                    l.DrugName,
                    l.BatchNumber,
                    l.ExpiryDate,
                    l.Quantity,
                    l.PurchasePrice)).ToList()))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public void Add(StockImport stockImport)
    {
        context.StockImports.Add(stockImport);
    }
}
