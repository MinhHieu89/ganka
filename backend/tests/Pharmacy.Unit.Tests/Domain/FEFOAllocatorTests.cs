using FluentAssertions;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Services;

namespace Pharmacy.Unit.Tests.Domain;

/// <summary>
/// Tests for the FEFOAllocator domain service.
/// FEFO = First Expiry, First Out - selects batches with the earliest expiry date first.
/// </summary>
public class FEFOAllocatorTests
{
    private static readonly Guid DrugId = Guid.NewGuid();
    private static readonly Guid SupplierId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Helper to create a DrugBatch with future expiry date.</summary>
    private static DrugBatch CreateBatch(string batchNumber, int quantity, int expiryDaysFromNow = 30)
    {
        return DrugBatch.Create(
            DrugId,
            SupplierId,
            batchNumber,
            Today.AddDays(expiryDaysFromNow),
            quantity,
            purchasePrice: 10_000m);
    }

    [Fact]
    public void Allocate_SingleBatch_WithSufficientStock_AllocatesFromIt()
    {
        // Arrange
        var batch = CreateBatch("B001", quantity: 100);
        var batches = new List<DrugBatch> { batch };

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 50);

        // Assert
        result.Should().HaveCount(1);
        result[0].BatchId.Should().Be(batch.Id);
        result[0].BatchNumber.Should().Be("B001");
        result[0].Quantity.Should().Be(50);
    }

    [Fact]
    public void Allocate_MultipleBatches_AllocatesFromEarliestExpiryFirst()
    {
        // Arrange: batch B-LATE expires later, B-EARLY expires sooner
        var earlyBatch = CreateBatch("B-EARLY", quantity: 100, expiryDaysFromNow: 10);
        var lateBatch = CreateBatch("B-LATE", quantity: 100, expiryDaysFromNow: 60);

        // Deliberately pass in reverse order to confirm ordering is applied
        var batches = new List<DrugBatch> { lateBatch, earlyBatch };

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 30);

        // Assert: should allocate from earliest expiry (B-EARLY) first
        result.Should().HaveCount(1);
        result[0].BatchNumber.Should().Be("B-EARLY");
        result[0].Quantity.Should().Be(30);
    }

    [Fact]
    public void Allocate_SpansMultipleBatches_WhenFirstInsufficient()
    {
        // Arrange: B-EARLY has only 20, need 50
        var earlyBatch = CreateBatch("B-EARLY", quantity: 20, expiryDaysFromNow: 10);
        var midBatch = CreateBatch("B-MID", quantity: 20, expiryDaysFromNow: 20);
        var lateBatch = CreateBatch("B-LATE", quantity: 100, expiryDaysFromNow: 60);

        var batches = new List<DrugBatch> { lateBatch, midBatch, earlyBatch };

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 50);

        // Assert: takes 20 from B-EARLY, 20 from B-MID, 10 from B-LATE
        result.Should().HaveCount(3);
        result[0].BatchNumber.Should().Be("B-EARLY");
        result[0].Quantity.Should().Be(20);
        result[1].BatchNumber.Should().Be("B-MID");
        result[1].Quantity.Should().Be(20);
        result[2].BatchNumber.Should().Be("B-LATE");
        result[2].Quantity.Should().Be(10);
    }

    [Fact]
    public void Allocate_SkipsExpiredBatches()
    {
        // Arrange: one expired batch, one valid batch
        var expiredBatch = CreateBatch("B-EXPIRED", quantity: 100, expiryDaysFromNow: -1);
        var validBatch = CreateBatch("B-VALID", quantity: 100, expiryDaysFromNow: 30);

        var batches = new List<DrugBatch> { expiredBatch, validBatch };

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 10);

        // Assert: only valid batch used
        result.Should().HaveCount(1);
        result[0].BatchNumber.Should().Be("B-VALID");
    }

    [Fact]
    public void Allocate_SkipsZeroQuantityBatches()
    {
        // Arrange: one empty batch, one batch with stock
        // Note: DrugBatch.Create requires quantity > 0, so we test via Deduct
        var emptyBatch = CreateBatch("B-EMPTY", quantity: 5, expiryDaysFromNow: 10);
        emptyBatch.Deduct(5); // deplete to zero
        var stockBatch = CreateBatch("B-STOCK", quantity: 100, expiryDaysFromNow: 30);

        var batches = new List<DrugBatch> { emptyBatch, stockBatch };

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 10);

        // Assert: zero-quantity batch skipped
        result.Should().HaveCount(1);
        result[0].BatchNumber.Should().Be("B-STOCK");
    }

    [Fact]
    public void Allocate_ReturnsEmpty_WhenInsufficientStock()
    {
        // Arrange: batches total only 30, but 50 required
        var batch1 = CreateBatch("B001", quantity: 10, expiryDaysFromNow: 10);
        var batch2 = CreateBatch("B002", quantity: 20, expiryDaysFromNow: 20);

        var batches = new List<DrugBatch> { batch1, batch2 };

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 50);

        // Assert: insufficient stock returns empty list (all-or-nothing)
        result.Should().BeEmpty();
    }

    [Fact]
    public void Allocate_ExactMatch_AllocatesAll()
    {
        // Arrange: request exactly what is available
        var batch = CreateBatch("B001", quantity: 25, expiryDaysFromNow: 30);

        var batches = new List<DrugBatch> { batch };

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 25);

        // Assert: allocates exact quantity
        result.Should().HaveCount(1);
        result[0].Quantity.Should().Be(25);
    }

    [Fact]
    public void Allocate_EmptyBatchList_ReturnsEmpty()
    {
        // Arrange
        var batches = new List<DrugBatch>();

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Allocate_BatchExpiryDateIsToday_TreatedAsExpired()
    {
        // Arrange: ExpiryDate == Today means expired (not valid today)
        // We use expiryDaysFromNow: 0 but Create doesn't restrict that
        // Need to verify IsExpired logic: ExpiryDate <= today is expired
        var todayBatch = DrugBatch.Create(DrugId, SupplierId, "B-TODAY", Today, 100, 10_000m);
        var futureBatch = CreateBatch("B-FUTURE", quantity: 50, expiryDaysFromNow: 1);

        var batches = new List<DrugBatch> { todayBatch, futureBatch };

        // Act
        var result = FEFOAllocator.Allocate(batches, requiredQuantity: 20);

        // Assert: today's expiry is treated as expired, only future batch used
        result.Should().HaveCount(1);
        result[0].BatchNumber.Should().Be("B-FUTURE");
    }
}
