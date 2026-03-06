using System.Globalization;

namespace Billing.Infrastructure.Documents.Shared;

/// <summary>
/// Utility for formatting Vietnamese Dong (VND) amounts.
/// Provides dot-separated thousands format (e.g., 1.500.000) and number-to-words conversion.
/// </summary>
public static class VndFormatter
{
    private static readonly CultureInfo ViCulture = new("vi-VN");

    /// <summary>
    /// Formats a decimal amount as VND with dot thousands separator and no decimals.
    /// Example: 1500000 -> "1.500.000"
    /// </summary>
    public static string FormatAmount(decimal amount)
    {
        return ((long)Math.Round(amount)).ToString("N0", ViCulture);
    }

    /// <summary>
    /// Formats a decimal amount as VND with currency suffix.
    /// Example: 1500000 -> "1.500.000 VND"
    /// </summary>
    public static string FormatAmountWithCurrency(decimal amount)
    {
        return $"{FormatAmount(amount)} VND";
    }

    /// <summary>
    /// Converts a VND amount to Vietnamese words for e-invoice compliance.
    /// Example: 1500000 -> "Mot trieu nam tram nghin dong"
    /// Handles up to billions (ty) which covers all realistic clinic invoices.
    /// </summary>
    public static string AmountToWords(decimal amount)
    {
        var rounded = (long)Math.Round(amount);

        if (rounded == 0)
            return "Kh\u00f4ng \u0111\u1ed3ng";

        var isNegative = rounded < 0;
        rounded = Math.Abs(rounded);

        var result = ConvertNumberToWords(rounded);

        // Capitalize first letter
        if (result.Length > 0)
            result = char.ToUpper(result[0]) + result[1..];

        if (isNegative)
            result = $"\u00c2m {result.ToLower()}";

        return $"{result} \u0111\u1ed3ng";
    }

    private static string ConvertNumberToWords(long number)
    {
        if (number == 0) return "kh\u00f4ng";

        var parts = new List<string>();

        // Ty (billion)
        if (number >= 1_000_000_000)
        {
            parts.Add($"{ConvertNumberToWords(number / 1_000_000_000)} t\u1ef7");
            number %= 1_000_000_000;
            if (number > 0 && number < 100_000_000)
                parts.Add("kh\u00f4ng tr\u0103m");
        }

        // Trieu (million)
        if (number >= 1_000_000)
        {
            parts.Add($"{ConvertHundreds((int)(number / 1_000_000))} tri\u1ec7u");
            number %= 1_000_000;
            if (number > 0 && number < 100_000)
                parts.Add("kh\u00f4ng tr\u0103m");
        }

        // Nghin (thousand)
        if (number >= 1_000)
        {
            parts.Add($"{ConvertHundreds((int)(number / 1_000))} ngh\u00ecn");
            number %= 1_000;
            if (number > 0 && number < 100)
                parts.Add("kh\u00f4ng tr\u0103m");
        }

        // Hundreds, tens, ones
        if (number > 0)
        {
            parts.Add(ConvertHundreds((int)number));
        }

        return string.Join(" ", parts);
    }

    private static string ConvertHundreds(int number)
    {
        var parts = new List<string>();

        if (number >= 100)
        {
            parts.Add($"{GetDigitWord(number / 100)} tr\u0103m");
            number %= 100;
            if (number > 0 && number < 10)
                parts.Add("l\u1ebb");
        }

        if (number >= 10)
        {
            if (number >= 20)
            {
                parts.Add($"{GetDigitWord(number / 10)} m\u01b0\u01a1i");
                number %= 10;
            }
            else if (number >= 10)
            {
                parts.Add("m\u01b0\u1eddi");
                number %= 10;
            }
        }

        if (number > 0)
        {
            if (number == 1 && parts.Count > 0)
                parts.Add("m\u1ed9t");
            else if (number == 5 && parts.Count > 0)
                parts.Add("l\u0103m");
            else
                parts.Add(GetDigitWord(number));
        }

        return string.Join(" ", parts);
    }

    private static string GetDigitWord(int digit) => digit switch
    {
        0 => "kh\u00f4ng",
        1 => "m\u1ed9t",
        2 => "hai",
        3 => "ba",
        4 => "b\u1ed1n",
        5 => "n\u0103m",
        6 => "s\u00e1u",
        7 => "b\u1ea3y",
        8 => "t\u00e1m",
        9 => "ch\u00edn",
        _ => ""
    };

    /// <summary>
    /// Gets the Vietnamese display name for a payment method.
    /// </summary>
    public static string GetPaymentMethodName(Domain.Enums.PaymentMethod method) => method switch
    {
        Domain.Enums.PaymentMethod.Cash => "Ti\u1ec1n m\u1eb7t",
        Domain.Enums.PaymentMethod.BankTransfer => "Chuy\u1ec3n kho\u1ea3n",
        Domain.Enums.PaymentMethod.QrVnPay => "QR VNPay",
        Domain.Enums.PaymentMethod.QrMomo => "QR MoMo",
        Domain.Enums.PaymentMethod.QrZaloPay => "QR ZaloPay",
        Domain.Enums.PaymentMethod.CardVisa => "Th\u1ebb Visa",
        Domain.Enums.PaymentMethod.CardMastercard => "Th\u1ebb Mastercard",
        _ => method.ToString()
    };

    /// <summary>
    /// Gets the Vietnamese display name for a department.
    /// </summary>
    public static string GetDepartmentName(Domain.Enums.Department department) => department switch
    {
        Domain.Enums.Department.Medical => "KH\u00c1M B\u1ec6NH",
        Domain.Enums.Department.Pharmacy => "D\u01af\u1ee2C PH\u1ea8M",
        Domain.Enums.Department.Optical => "K\u00cdNH",
        Domain.Enums.Department.Treatment => "\u0110I\u1ec0U TR\u1eca",
        _ => department.ToString().ToUpperInvariant()
    };
}
