namespace Pharmacy.Contracts.Dtos;

/// <summary>
/// Cross-module DTO for DrugBatch entity.
/// Represents a specific lot/batch of a drug with expiry and quantity tracking.
/// IsExpired and IsNearExpiry are computed fields from the backend.
/// </summary>
public sealed record DrugBatchDto(
    Guid Id,
    Guid DrugCatalogItemId,
    Guid SupplierId,
    string SupplierName,
    string BatchNumber,
    DateOnly ExpiryDate,
    int InitialQuantity,
    int CurrentQuantity,
    decimal PurchasePrice,
    bool IsExpired,
    bool IsNearExpiry);

/// <summary>
/// Cross-module DTO for drug inventory summary.
/// Aggregates batch data with catalog info for inventory list views.
/// Form and Route are int-serialized enums (Pharmacy.Domain.Enums.DrugForm / DrugRoute)
/// because Contracts does not reference Domain.
/// </summary>
public sealed record DrugInventoryDto(
    Guid DrugCatalogItemId,
    string Name,
    string NameVi,
    string GenericName,
    string Unit,
    int Form,
    int Route,
    decimal? SellingPrice,
    int MinStockLevel,
    int TotalStock,
    int BatchCount,
    bool IsLowStock,
    bool HasExpiryAlert,
    bool IsOutOfStock);

/// <summary>
/// Cross-module DTO for a stock import event (supplier invoice or Excel bulk import).
/// </summary>
public sealed record StockImportDto(
    Guid Id,
    Guid SupplierId,
    string SupplierName,
    int ImportSource,
    string? InvoiceNumber,
    DateTime ImportedAt,
    string? Notes,
    List<StockImportLineDto> Lines);

/// <summary>
/// Cross-module DTO for a single line in a stock import.
/// </summary>
public sealed record StockImportLineDto(
    Guid Id,
    Guid DrugCatalogItemId,
    string DrugName,
    string BatchNumber,
    DateOnly ExpiryDate,
    int Quantity,
    decimal PurchasePrice);

/// <summary>
/// Cross-module DTO for a manual stock adjustment record.
/// Reason is int-serialized Pharmacy.Domain.Enums.StockAdjustmentReason.
/// Either DrugBatchId or ConsumableBatchId is non-null (never both).
/// </summary>
public sealed record StockAdjustmentDto(
    Guid Id,
    Guid? DrugBatchId,
    Guid? ConsumableBatchId,
    int QuantityChange,
    int Reason,
    string? Notes,
    Guid AdjustedById,
    DateTime AdjustedAt);

/// <summary>
/// Cross-module DTO for expiry alert on a specific drug batch.
/// </summary>
public sealed record ExpiryAlertDto(
    Guid DrugCatalogItemId,
    string DrugName,
    string BatchNumber,
    DateOnly ExpiryDate,
    int CurrentQuantity,
    int DaysUntilExpiry);

/// <summary>
/// Cross-module DTO for low stock alert on a specific drug.
/// </summary>
public sealed record LowStockAlertDto(
    Guid DrugCatalogItemId,
    string DrugName,
    int TotalStock,
    int MinStockLevel);
