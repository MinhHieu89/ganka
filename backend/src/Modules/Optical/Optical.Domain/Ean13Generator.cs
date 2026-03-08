namespace Optical.Domain;

/// <summary>
/// Generates and validates EAN-13 barcodes for optical frame catalog items.
///
/// EAN-13 structure: 12 payload digits + 1 check digit calculated using the standard
/// Luhn-style modulo-10 algorithm (alternating weights 1 and 3 from left to right).
///
/// Clinic-generated barcodes use a configurable 3-digit prefix (default "200")
/// that falls within the GS1 internal-use range (200-299), ensuring no conflict with
/// manufacturer barcodes.
/// </summary>
public static class Ean13Generator
{
    /// <summary>
    /// Default clinic prefix for auto-generated barcodes.
    /// GS1 range 200-299 is reserved for internal/in-store use.
    /// </summary>
    private const string DefaultPrefix = "200";

    /// <summary>
    /// Generates a unique EAN-13 barcode using the clinic prefix and a sequential or random payload.
    /// </summary>
    /// <param name="prefix">3-digit GS1 prefix. Defaults to "200" (internal use range).</param>
    /// <returns>13-character EAN-13 barcode string with valid check digit.</returns>
    public static string Generate(string? prefix = null)
    {
        var effectivePrefix = prefix ?? DefaultPrefix;

        if (effectivePrefix.Length != 3 || !effectivePrefix.All(char.IsDigit))
            throw new ArgumentException("EAN-13 prefix must be exactly 3 numeric digits.", nameof(prefix));

        // Generate 9 random digits to form the 12-digit payload (3 prefix + 9 random)
        var random = new Random();
        var payload = effectivePrefix + string.Concat(
            Enumerable.Range(0, 9).Select(_ => random.Next(0, 10).ToString()));

        var checkDigit = CalculateCheckDigit(payload);
        return payload + checkDigit;
    }

    /// <summary>
    /// Validates whether the given string is a valid EAN-13 barcode.
    /// Checks both length (must be 13 digits) and check digit correctness.
    /// </summary>
    /// <param name="barcode">Barcode string to validate.</param>
    /// <returns>True if the barcode is exactly 13 digits with a correct check digit.</returns>
    public static bool IsValid(string? barcode)
    {
        if (barcode is null || barcode.Length != 13 || !barcode.All(char.IsDigit))
            return false;

        var payload = barcode[..12];
        var expectedCheckDigit = CalculateCheckDigit(payload);
        return barcode[12].ToString() == expectedCheckDigit;
    }

    /// <summary>
    /// Calculates the EAN-13 check digit for a 12-digit payload using the standard
    /// modulo-10 algorithm: sum of digits with alternating weights 1 (odd positions)
    /// and 3 (even positions, 0-indexed), then (10 - sum % 10) % 10.
    /// </summary>
    /// <param name="payload">12-digit payload string (without check digit).</param>
    /// <returns>Single-character string representing the check digit (0-9).</returns>
    /// <exception cref="ArgumentException">Thrown when payload is not exactly 12 numeric digits.</exception>
    public static string CalculateCheckDigit(string payload)
    {
        if (payload.Length != 12 || !payload.All(char.IsDigit))
            throw new ArgumentException("Payload must be exactly 12 numeric digits.", nameof(payload));

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = payload[i] - '0';
            // EAN-13: odd positions (0, 2, 4, ...) weight 1; even positions (1, 3, 5, ...) weight 3
            sum += i % 2 == 0 ? digit : digit * 3;
        }

        var checkDigit = (10 - sum % 10) % 10;
        return checkDigit.ToString();
    }
}
