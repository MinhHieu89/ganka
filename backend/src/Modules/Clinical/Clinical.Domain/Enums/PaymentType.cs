namespace Clinical.Domain.Enums;

/// <summary>
/// Type of payment associated with a visit.
/// Visit = exam/drug payment, Glasses = optical order payment.
/// In the single-payment model, both are combined at Cashier.
/// </summary>
public enum PaymentType
{
    Visit = 0,
    Glasses = 1
}
