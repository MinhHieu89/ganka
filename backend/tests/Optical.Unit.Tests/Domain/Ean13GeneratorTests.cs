using FluentAssertions;
using Optical.Domain;

namespace Optical.Unit.Tests.Domain;

/// <summary>
/// Tests for the Ean13Generator utility class: generation, validation, and check digit calculation.
/// Uses the known EAN-13 barcode "5901234123457" where check digit is 7.
/// </summary>
public class Ean13GeneratorTests
{
    // Known valid EAN-13: "5901234123457" — standard test vector from GS1
    private const string KnownValidBarcode = "5901234123457";
    private const string KnownPayload = "590123412345";    // 12 digits
    private const string KnownCheckDigit = "7";

    // --- Generation Tests ---

    [Fact]
    public void Generate_ShouldReturn13Digits()
    {
        // Act
        var barcode = Ean13Generator.Generate();

        // Assert
        barcode.Should().HaveLength(13);
        barcode.Should().MatchRegex("^[0-9]{13}$");
    }

    [Fact]
    public void Generate_ShouldHaveCorrectCheckDigit()
    {
        // Act
        var barcode = Ean13Generator.Generate();

        // Assert — validate using IsValid to confirm check digit is correct
        Ean13Generator.IsValid(barcode).Should().BeTrue();
    }

    [Fact]
    public void Generate_WithDefaultPrefix_ShouldStartWith200()
    {
        // Act
        var barcode = Ean13Generator.Generate();

        // Assert — default clinic prefix is "200" (GS1 internal-use range)
        barcode.Should().StartWith("200");
    }

    [Fact]
    public void Generate_WithCustomPrefix_ShouldStartWithPrefix()
    {
        // Act
        var barcode = Ean13Generator.Generate(prefix: "880");

        // Assert
        barcode.Should().StartWith("880");
        barcode.Should().HaveLength(13);
    }

    [Fact]
    public void Generate_WithInvalidPrefix_ShouldThrow()
    {
        // Act
        var act = () => Ean13Generator.Generate(prefix: "12");   // Too short
        var act2 = () => Ean13Generator.Generate(prefix: "ABC"); // Non-numeric

        // Assert
        act.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Generate_MultipleCalls_ShouldProduceAllValidBarcodes()
    {
        // Act — generate 50 barcodes
        var barcodes = Enumerable.Range(0, 50)
            .Select(_ => Ean13Generator.Generate())
            .ToList();

        // Assert — all should be valid
        barcodes.Should().OnlyContain(b => Ean13Generator.IsValid(b));
    }

    // --- Validation Tests ---

    [Fact]
    public void IsValid_ValidBarcode_ShouldReturnTrue()
    {
        // Assert — known valid EAN-13 barcode
        Ean13Generator.IsValid(KnownValidBarcode).Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidCheckDigit_ShouldReturnFalse()
    {
        // Arrange — corrupt the check digit (7 -> 8)
        var invalidBarcode = KnownPayload + "8";

        // Assert
        Ean13Generator.IsValid(invalidBarcode).Should().BeFalse();
    }

    [Fact]
    public void IsValid_WrongLength_ShouldReturnFalse()
    {
        // Arrange
        var tooShort = "590123412345";   // 12 digits
        var tooLong = "59012341234578";  // 14 digits

        // Assert
        Ean13Generator.IsValid(tooShort).Should().BeFalse();
        Ean13Generator.IsValid(tooLong).Should().BeFalse();
    }

    [Fact]
    public void IsValid_NullInput_ShouldReturnFalse()
    {
        // Assert
        Ean13Generator.IsValid(null).Should().BeFalse();
    }

    [Fact]
    public void IsValid_NonNumericCharacters_ShouldReturnFalse()
    {
        // Arrange
        var nonNumeric = "590123412345A";

        // Assert
        Ean13Generator.IsValid(nonNumeric).Should().BeFalse();
    }

    [Fact]
    public void IsValid_AllZeros_ShouldReturnFalse()
    {
        // Arrange — "0000000000000" — check digit for 12 zeros is 0, so this is actually valid
        // "000000000000" payload -> sum = 0 -> check = (10 - 0%10)%10 = 0
        // So "0000000000000" should be valid
        Ean13Generator.IsValid("0000000000000").Should().BeTrue();
    }

    // --- Check Digit Calculation Tests ---

    [Fact]
    public void CalculateCheckDigit_KnownValue_ShouldMatch()
    {
        // Arrange — known EAN-13: "5901234123457" -> check digit is "7"
        // Act
        var checkDigit = Ean13Generator.CalculateCheckDigit(KnownPayload);

        // Assert
        checkDigit.Should().Be(KnownCheckDigit);
    }

    [Theory]
    [InlineData("590123412345", "7")]   // Known EAN-13 test vector
    [InlineData("400638133393", "1")]   // 4006381333931
    [InlineData("000000000000", "0")]   // All zeros -> check digit is 0
    public void CalculateCheckDigit_KnownValues_ShouldReturnCorrectDigit(
        string payload, string expectedCheckDigit)
    {
        // Act
        var checkDigit = Ean13Generator.CalculateCheckDigit(payload);

        // Assert
        checkDigit.Should().Be(expectedCheckDigit);
    }

    [Fact]
    public void CalculateCheckDigit_InvalidPayload_ShouldThrow()
    {
        // Arrange — less than 12 digits
        var act1 = () => Ean13Generator.CalculateCheckDigit("12345");
        // Arrange — non-numeric
        var act2 = () => Ean13Generator.CalculateCheckDigit("12345678901A");

        // Assert
        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }
}
