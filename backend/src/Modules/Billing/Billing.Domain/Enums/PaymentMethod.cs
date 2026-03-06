namespace Billing.Domain.Enums;

/// <summary>
/// Payment method types accepted by the clinic.
/// </summary>
public enum PaymentMethod
{
    Cash = 0,
    BankTransfer = 1,
    QrVnPay = 2,
    QrMomo = 3,
    QrZaloPay = 4,
    CardVisa = 5,
    CardMastercard = 6
}

/// <summary>
/// Discount type: percentage off or fixed VND amount.
/// </summary>
public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1
}
