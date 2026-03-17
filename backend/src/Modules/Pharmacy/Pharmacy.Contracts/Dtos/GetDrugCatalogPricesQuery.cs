namespace Pharmacy.Contracts.Dtos;

/// <summary>
/// Query record for looking up drug catalog prices by catalog item IDs.
/// Invokable from the Billing module via IMessageBus to get pricing data
/// without coupling Billing to Pharmacy domain.
/// </summary>
public sealed record GetDrugCatalogPricesQuery(List<Guid> CatalogItemIds);

/// <summary>
/// Response DTO for drug catalog price lookup.
/// </summary>
public sealed record DrugCatalogPriceDto(
    Guid CatalogItemId,
    decimal SellingPrice,
    string? NameVi);
