namespace Billing.Domain.Enums;

/// <summary>
/// Status of an invoice in its lifecycle.
/// Draft = editable, Finalized = locked and paid, Voided = cancelled.
/// </summary>
public enum InvoiceStatus
{
    Draft = 0,
    Finalized = 1,
    Voided = 2
}
