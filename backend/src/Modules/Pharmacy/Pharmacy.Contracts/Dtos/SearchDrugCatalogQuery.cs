namespace Pharmacy.Contracts.Dtos;

/// <summary>
/// Query record for searching the drug catalog via Wolverine IMessageBus.
/// The Clinical module invokes this query to search for drugs during prescription writing.
/// Handled by Pharmacy.Application.
/// </summary>
public sealed record SearchDrugCatalogQuery(string SearchTerm);
