namespace Billing.Domain.Enums;

/// <summary>
/// Lifecycle status of an invoice.
/// </summary>
public enum InvoiceStatus
{
    Draft = 0,
    Finalized = 1,
    Voided = 2
}
