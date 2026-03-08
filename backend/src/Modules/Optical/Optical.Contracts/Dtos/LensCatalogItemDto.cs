namespace Optical.Contracts.Dtos;

/// <summary>
/// Full lens catalog item DTO for API serialization.
/// Enum fields are int-serialized per established Billing pattern:
///   Material → Optical.Domain.Enums.LensMaterial
///   AvailableCoatings → Optical.Domain.Enums.LensCoating ([Flags] bitwise combination)
/// </summary>
public sealed record LensCatalogItemDto(
    Guid Id,
    string Brand,
    string Name,
    string LensType,
    int Material,
    int AvailableCoatings,
    decimal SellingPrice,
    decimal CostPrice,
    bool IsActive,
    Guid? PreferredSupplierId,
    string? SupplierName,
    List<LensStockEntryDto> StockEntries,
    DateTime CreatedAt);

/// <summary>
/// Individual lens stock entry for a specific power combination.
/// Sph and Cyl represent spherical and cylindrical power in diopters.
/// Add represents addition power for progressive/bifocal lenses.
/// </summary>
public sealed record LensStockEntryDto(
    Guid Id,
    Guid LensCatalogItemId,
    decimal Sph,
    decimal Cyl,
    decimal? Add,
    int Quantity,
    int MinStockLevel);

/// <summary>
/// Custom lens order placed with a supplier for a specific patient prescription.
/// Status is a string representation of the lens order lifecycle state.
/// </summary>
public sealed record LensOrderDto(
    Guid Id,
    Guid LensCatalogItemId,
    Guid SupplierId,
    string? SupplierName,
    Guid GlassesOrderId,
    Guid PatientId,
    decimal Sph,
    decimal Cyl,
    decimal? Add,
    decimal? Axis,
    int RequestedCoatings,
    string Status,
    DateTime? ReceivedAt,
    string? Notes,
    DateTime CreatedAt);
