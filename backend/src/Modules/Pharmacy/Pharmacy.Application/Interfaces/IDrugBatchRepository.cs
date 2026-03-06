using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

/// <summary>
/// Repository interface for DrugBatch persistence and FEFO (First Expired, First Out) queries.
/// Critical for batch selection during dispensing and OTC sales.
/// </summary>
public interface IDrugBatchRepository
{
    /// <summary>
    /// Gets a drug batch by ID (returns domain entity for mutation).
    /// </summary>
    Task<DrugBatch?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all batches for a specific drug catalog item, including empty batches.
    /// Use for audit/history purposes.
    /// </summary>
    Task<List<DrugBatch>> GetBatchesForDrugAsync(Guid drugCatalogItemId, CancellationToken ct);

    /// <summary>
    /// Gets available batches for FEFO dispensing: CurrentQuantity > 0, not expired,
    /// ordered by ExpiryDate ASC (earliest expiry first).
    /// </summary>
    Task<List<DrugBatch>> GetAvailableBatchesFEFOAsync(Guid drugCatalogItemId, CancellationToken ct);

    /// <summary>
    /// Gets the total available stock for a drug by summing CurrentQuantity across all
    /// non-expired, non-empty batches.
    /// </summary>
    Task<int> GetTotalStockAsync(Guid drugCatalogItemId, CancellationToken ct);

    /// <summary>
    /// Gets batches expiring within the specified number of days.
    /// Used for expiry alert notifications and inventory reports.
    /// </summary>
    Task<List<ExpiryAlertDto>> GetExpiryAlertsAsync(int daysThreshold, CancellationToken ct);

    /// <summary>
    /// Gets drugs whose total available stock is at or below MinStockLevel.
    /// Used for low stock alert notifications.
    /// </summary>
    Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(CancellationToken ct);

    /// <summary>
    /// Adds a new drug batch to the change tracker.
    /// </summary>
    void Add(DrugBatch batch);

    /// <summary>
    /// Adds a new StockAdjustment to the change tracker.
    /// </summary>
    void AddStockAdjustment(StockAdjustment adjustment);
}
