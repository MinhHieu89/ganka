namespace Billing.Domain.Enums;

/// <summary>
/// Method used to collect payment from the patient.
/// Covers cash, bank transfer, QR-based mobile wallets, and card payments.
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
/// Type of discount applied to an invoice or line item.
/// Percentage discounts are relative (0-100), FixedAmount is an absolute VND value.
/// </summary>
public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1
}
