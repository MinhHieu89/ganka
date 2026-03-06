using FluentAssertions;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Unit.Tests.Domain;

/// <summary>
/// Tests for DrugBatch domain entity methods: Deduct, IsExpired, IsNearExpiry.
/// </summary>
public class DrugBatchTests
{
    private static readonly Guid DrugId = Guid.NewGuid();
    private static readonly Guid SupplierId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private static DrugBatch CreateBatch(int quantity = 100, int expiryDaysFromNow = 30) =>
        DrugBatch.Create(
            DrugId,
            SupplierId,
            "BN2024001",
            Today.AddDays(expiryDaysFromNow),
            quantity,
            purchasePrice: 50_000m);

    // --- Deduct Tests ---

    [Fact]
    public void Deduct_ValidQuantity_ReducesCurrentQuantity()
    {
        // Arrange
        var batch = CreateBatch(quantity: 100);

        // Act
        batch.Deduct(30);

        // Assert
        batch.CurrentQuantity.Should().Be(70);
    }

    [Fact]
    public void Deduct_ExactQuantity_ReducesToZero()
    {
        // Arrange
        var batch = CreateBatch(quantity: 50);

        // Act
        batch.Deduct(50);

        // Assert
        batch.CurrentQuantity.Should().Be(0);
    }

    [Fact]
    public void Deduct_QuantityExceedsStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var batch = CreateBatch(quantity: 10);

        // Act & Assert
        var act = () => batch.Deduct(11);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*10*"); // Message should mention available quantity
    }

    [Fact]
    public void Deduct_ZeroQuantity_ThrowsArgumentException()
    {
        // Arrange
        var batch = CreateBatch(quantity: 100);

        // Act & Assert
        var act = () => batch.Deduct(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deduct_NegativeQuantity_ThrowsArgumentException()
    {
        // Arrange
        var batch = CreateBatch(quantity: 100);

        // Act & Assert
        var act = () => batch.Deduct(-5);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deduct_MultipleDeductions_AccumulateCorrectly()
    {
        // Arrange
        var batch = CreateBatch(quantity: 100);

        // Act
        batch.Deduct(20);
        batch.Deduct(30);

        // Assert
        batch.CurrentQuantity.Should().Be(50);
    }

    // --- IsExpired Tests ---

    [Fact]
    public void IsExpired_FutureExpiry_ReturnsFalse()
    {
        // Arrange
        var batch = CreateBatch(expiryDaysFromNow: 30);

        // Act & Assert
        batch.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ExpiryIsToday_ReturnsTrue()
    {
        // Arrange: ExpiryDate == Today means expired (today's stock should not be dispensed)
        var batch = DrugBatch.Create(DrugId, SupplierId, "BN001", Today, 100, 10_000m);

        // Act & Assert
        batch.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_PastExpiry_ReturnsTrue()
    {
        // Arrange
        var batch = CreateBatch(expiryDaysFromNow: -1);

        // Act & Assert
        batch.IsExpired.Should().BeTrue();
    }

    // --- IsNearExpiry Tests ---

    [Fact]
    public void IsNearExpiry_ExpiresWithinThreshold_ReturnsTrue()
    {
        // Arrange: expires in 15 days, threshold is 30 days
        var batch = CreateBatch(expiryDaysFromNow: 15);

        // Act & Assert
        batch.IsNearExpiry(daysThreshold: 30).Should().BeTrue();
    }

    [Fact]
    public void IsNearExpiry_ExpiresAfterThreshold_ReturnsFalse()
    {
        // Arrange: expires in 60 days, threshold is 30 days
        var batch = CreateBatch(expiryDaysFromNow: 60);

        // Act & Assert
        batch.IsNearExpiry(daysThreshold: 30).Should().BeFalse();
    }

    [Fact]
    public void IsNearExpiry_AlreadyExpired_ReturnsFalse()
    {
        // Arrange: already expired batch should NOT be "near expiry"
        var batch = CreateBatch(expiryDaysFromNow: -5);

        // Act & Assert
        batch.IsNearExpiry(daysThreshold: 30).Should().BeFalse();
    }

    // --- AddStock Tests ---

    [Fact]
    public void AddStock_ValidQuantity_IncreasesCurrentQuantity()
    {
        // Arrange
        var batch = CreateBatch(quantity: 50);

        // Act
        batch.AddStock(25);

        // Assert
        batch.CurrentQuantity.Should().Be(75);
    }

    [Fact]
    public void AddStock_ZeroQuantity_ThrowsArgumentException()
    {
        // Arrange
        var batch = CreateBatch(quantity: 50);

        // Act & Assert
        var act = () => batch.AddStock(0);
        act.Should().Throw<ArgumentException>();
    }
}
