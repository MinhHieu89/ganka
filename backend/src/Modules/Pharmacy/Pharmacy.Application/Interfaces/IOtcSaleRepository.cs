using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

/// <summary>
/// Repository interface for OtcSale persistence operations.
/// Supports walk-in OTC sales without prescription.
/// </summary>
public interface IOtcSaleRepository
{
    /// <summary>
    /// Gets an OTC sale by ID (returns domain entity for mutation).
    /// </summary>
    Task<OtcSale?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets a paginated list of OTC sales with total count.
    /// </summary>
    Task<(List<OtcSaleDto> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Adds a new OTC sale to the change tracker.
    /// </summary>
    void Add(OtcSale sale);
}
