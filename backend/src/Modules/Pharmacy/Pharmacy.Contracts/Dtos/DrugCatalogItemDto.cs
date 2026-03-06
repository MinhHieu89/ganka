namespace Pharmacy.Contracts.Dtos;

/// <summary>
/// Cross-module DTO exposing drug catalog item data.
/// Used by the Clinical module to display drug information during prescription writing.
/// Form and Route are int-serialized enums (Pharmacy.Domain.Enums.DrugForm / DrugRoute)
/// because Contracts does not reference Domain.
/// Phase 6 additions: SellingPrice and MinStockLevel for inventory management.
/// </summary>
public sealed record DrugCatalogItemDto(
    Guid Id,
    string Name,
    string NameVi,
    string GenericName,
    int Form,
    string? Strength,
    int Route,
    string Unit,
    string? DefaultDosageTemplate,
    bool IsActive,
    decimal? SellingPrice = null,
    int MinStockLevel = 0);
