using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

/// <summary>
/// Repository interface for StockImport persistence operations.
/// </summary>
public interface IStockImportRepository
{
    /// <summary>
    /// Gets a stock import by ID with all its lines (returns domain entity for mutation).
    /// </summary>
    Task<StockImport?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets a paginated list of stock imports with total count.
    /// </summary>
    Task<(List<StockImportDto> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Adds a new stock import to the change tracker.
    /// </summary>
    void Add(StockImport stockImport);
}
