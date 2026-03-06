namespace Billing.Contracts.Queries;

/// <summary>
/// Cross-module query to retrieve the invoice associated with a specific visit.
/// Used by other modules (e.g., Clinical) to check billing status via IMessageBus.
/// Handled by Billing.Application.
/// </summary>
public sealed record GetVisitInvoiceQuery(Guid VisitId);

/// <summary>
/// Query to retrieve unpaid/pending invoices, optionally filtered by cashier shift.
/// Used in the cashier dashboard to display the payment queue.
/// </summary>
public sealed record GetPendingInvoicesQuery(Guid? CashierShiftId = null);

/// <summary>
/// Cross-module query for Billing to collect charges from Clinical, Pharmacy, and Optical modules.
/// Each module responds with its charges for the given visit.
/// Handled by the respective module's Application layer.
/// </summary>
public sealed record GetVisitChargesQuery(Guid VisitId);

/// <summary>
/// DTO representing a single charge item from a source module (Clinical, Pharmacy, Optical).
/// Department is int-serialized Billing.Domain.Enums.Department.
/// SourceType indicates the originating entity (e.g., "Examination", "Dispensing", "LensOrder").
/// </summary>
public sealed record VisitChargeDto(
    string Description,
    string? DescriptionVi,
    decimal UnitPrice,
    int Quantity,
    int Department,
    Guid SourceId,
    string SourceType);
