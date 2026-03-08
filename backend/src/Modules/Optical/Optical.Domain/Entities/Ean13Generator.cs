namespace Optical.Domain.Entities;

/// <summary>
/// Static utility for generating and validating EAN-13 barcodes for clinic-tagged frames.
/// Uses a configurable clinic prefix (default "8930000" — Vietnam GS1 "893" + placeholder company code).
/// Format: [7-digit prefix][5-digit sequence][1-digit check] = 13 digits total.
/// Supports up to 99,999 clinic-generated items per prefix configuration.
/// </summary>
/// <remarks>
/// For frames with manufacturer-supplied EAN-13 barcodes, use the barcode from the physical label directly
/// and validate it with <see cref="IsValid(string)"/> before persisting.
/// Only use <see cref="Generate(long)"/> for frames without a manufacturer barcode.
/// </remarks>
public static class Ean13Generator
{
    /// <summary>
    /// Default EAN-13 company prefix for clinic-generated barcodes.
    /// "893" = Vietnam GS1 country prefix + "0000" placeholder company code.
    /// Replace with the clinic's registered GS1 company prefix when available.
    /// </summary>
    public const string DefaultPrefix = "8930000";

    /// <summary>
    /// Generates a valid EAN-13 barcode for a clinic-tagged frame.
    /// </summary>
    /// <param name="sequenceNumber">
    /// Sequential item number (1–99,999). Determines the middle 5 digits of the barcode.
    /// </param>
    /// <returns>A 13-character string containing only digits, with a valid EAN-13 check digit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="sequenceNumber"/> is less than 1 or greater than 99,999.
    /// </exception>
    public static string Generate(long sequenceNumber)
        => GenerateWithPrefix(DefaultPrefix, sequenceNumber);

    /// <summary>
    /// Generates a valid EAN-13 barcode using a custom 7-digit prefix.
    /// </summary>
    /// <param name="prefix">Seven-digit clinic or company prefix (must be exactly 7 digits).</param>
    /// <param name="sequenceNumber">Sequential item number (1–99,999).</param>
    /// <returns>A 13-character digit string with a valid EAN-13 check digit.</returns>
    /// <exception cref="ArgumentException">Thrown when prefix is not exactly 7 digits.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when sequence number is out of range.</exception>
    public static string GenerateWithPrefix(string prefix, long sequenceNumber)
    {
        if (string.IsNullOrEmpty(prefix) || prefix.Length != 7 || !prefix.All(char.IsDigit))
            throw new ArgumentException(
                "Prefix must be exactly 7 numeric digits (e.g., \"8930000\").", nameof(prefix));

        if (sequenceNumber < 1 || sequenceNumber > 99_999)
            throw new ArgumentOutOfRangeException(
                nameof(sequenceNumber),
                sequenceNumber,
                "Sequence number must be between 1 and 99,999.");

        var first12 = $"{prefix}{sequenceNumber:D5}";
        var checkDigit = CalculateCheckDigit(first12);
        return $"{first12}{checkDigit}";
    }

    /// <summary>
    /// Calculates the EAN-13 check digit using the standard modulo-10 algorithm.
    /// Alternating weights: odd positions (1-indexed) weight 1, even positions weight 3.
    /// </summary>
    /// <param name="first12">The first 12 digits of the EAN-13 code (must be exactly 12 digits).</param>
    /// <returns>The check digit (0–9).</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="first12"/> is not exactly 12 numeric characters.
    /// </exception>
    public static int CalculateCheckDigit(string first12)
    {
        if (first12 is null || first12.Length != 12 || !first12.All(char.IsDigit))
            throw new ArgumentException(
                "Input must be exactly 12 numeric digits.", nameof(first12));

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = first12[i] - '0';
            // EAN-13 weights: positions 0,2,4,... (odd in 1-indexed notation) weight 1;
            // positions 1,3,5,... (even in 1-indexed notation) weight 3.
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        var remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }

    /// <summary>
    /// Validates a 13-digit EAN-13 barcode string by checking its format and check digit.
    /// </summary>
    /// <param name="barcode">The barcode string to validate.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="barcode"/> is exactly 13 numeric digits and has a valid check digit;
    /// <c>false</c> otherwise.
    /// </returns>
    public static bool IsValid(string barcode)
    {
        if (barcode is null || barcode.Length != 13 || !barcode.All(char.IsDigit))
            return false;

        var expected = CalculateCheckDigit(barcode[..12]);
        return (barcode[12] - '0') == expected;
    }
}
