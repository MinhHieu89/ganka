namespace Pharmacy.Contracts.Dtos;

/// <summary>
/// Cross-module DTO for Supplier entity.
/// Used for supplier management cross-module consumption.
/// </summary>
public sealed record SupplierDto(
    Guid Id,
    string Name,
    string? ContactInfo,
    string? Phone,
    string? Email,
    bool IsActive);

/// <summary>
/// Cross-module DTO for SupplierDrugPrice entity.
/// Represents the default purchase price a supplier offers for a specific drug.
/// </summary>
public sealed record SupplierDrugPriceDto(
    Guid Id,
    Guid SupplierId,
    string SupplierName,
    Guid DrugCatalogItemId,
    string DrugName,
    decimal DefaultPurchasePrice);
