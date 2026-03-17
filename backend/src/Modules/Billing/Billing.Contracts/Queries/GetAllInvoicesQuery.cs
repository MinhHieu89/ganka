using Billing.Contracts.Dtos;

namespace Billing.Contracts.Queries;

/// <summary>
/// Query to retrieve all invoices with optional status filter, search, and pagination.
/// Used by the Invoice History page to browse Draft, Finalized, and Voided invoices.
/// </summary>
public sealed record GetAllInvoicesQuery(
    int? Status,
    string? Search,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// Paginated result containing invoice summaries and total count for pagination.
/// </summary>
public sealed record PaginatedInvoicesResult(
    List<InvoiceSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
