namespace Billing.Domain.Enums;

/// <summary>
/// Lifecycle status of a cashier shift.
/// Locked is an intermediate state that prevents new payment assignments before final close.
/// </summary>
public enum ShiftStatus
{
    Open = 0,
    Locked = 1,
    Closed = 2
}
