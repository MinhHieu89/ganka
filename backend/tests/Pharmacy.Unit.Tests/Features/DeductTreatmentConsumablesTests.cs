using FluentAssertions;
using NSubstitute;
using Pharmacy.Application.Features.Consumables;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Domain;
using Treatment.Contracts.IntegrationEvents;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for DeductTreatmentConsumablesHandler.
/// Cross-module event handler: Pharmacy module responds to TreatmentSessionCompletedIntegrationEvent
/// and deducts consumable stock used during the treatment session (TRT-11).
///
/// Behavior:
/// - SimpleStock items: RemoveStock directly, deduct available if insufficient
/// - ExpiryTracked items: FEFO batch deduction, deduct available if insufficient
/// - Missing consumable item: log warning, continue with remaining items
/// - Empty consumables list: no-op
/// - Multiple consumables: deducts all in single transaction
/// </summary>
public class DeductTreatmentConsumablesTests
{
    private readonly IConsumableRepository _repository = Substitute.For<IConsumableRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultPackageId = Guid.NewGuid();
    private static readonly Guid DefaultSessionId = Guid.NewGuid();
    private static readonly Guid DefaultPatientId = Guid.NewGuid();

    private static ConsumableItem CreateSimpleStockItem(Guid id, int currentStock = 10)
    {
        var item = ConsumableItem.Create(
            "Eye Shield",
            "Kinh che mat",
            "Piece",
            ConsumableTrackingMode.SimpleStock,
            minStockLevel: 5,
            new BranchId(DefaultBranchId));

        // Set the Id via reflection
        var idProp = typeof(Entity).GetProperty("Id");
        idProp?.GetSetMethod(true)?.Invoke(item, [id]);

        // Add stock to set initial quantity
        if (currentStock > 0)
            item.AddStock(currentStock);

        return item;
    }

    private static ConsumableItem CreateExpiryTrackedItem(Guid id)
    {
        var item = ConsumableItem.Create(
            "IPL Gel",
            "Gel IPL",
            "Tube",
            ConsumableTrackingMode.ExpiryTracked,
            minStockLevel: 2,
            new BranchId(DefaultBranchId));

        var idProp = typeof(Entity).GetProperty("Id");
        idProp?.GetSetMethod(true)?.Invoke(item, [id]);

        return item;
    }

    private static ConsumableBatch CreateBatch(Guid consumableItemId, int quantity, DateOnly expiryDate, string batchNumber = "CB001")
    {
        return ConsumableBatch.Create(
            consumableItemId: consumableItemId,
            batchNumber: batchNumber,
            expiryDate: expiryDate,
            quantity: quantity);
    }

    private static TreatmentSessionCompletedIntegrationEvent CreateEvent(
        List<TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto>? consumables = null)
    {
        return new TreatmentSessionCompletedIntegrationEvent(
            PackageId: DefaultPackageId,
            SessionId: DefaultSessionId,
            PatientId: DefaultPatientId,
            PatientName: "Test Patient",
            TreatmentType: 0, // IPL
            Consumables: consumables ?? [],
            VisitId: null,
            SessionFeeAmount: 0m,
            BranchId: DefaultBranchId);
    }

    #region Valid Deduction Tests

    [Fact]
    public async Task Handle_SimpleStockItem_DeductsQuantity()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = CreateSimpleStockItem(itemId, currentStock: 20);

        _repository.GetByIdAsync(itemId, Arg.Any<CancellationToken>()).Returns(item);

        var message = CreateEvent([
            new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(itemId, 5)
        ]);

        // Act
        await DeductTreatmentConsumablesHandler.Handle(
            message, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        item.CurrentStock.Should().Be(15); // 20 - 5
        _repository.Received(1).Update(item);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExpiryTrackedItem_DeductsFEFO()
    {
        // Arrange: two batches, earliest-expiry first should be deducted
        var itemId = Guid.NewGuid();
        var item = CreateExpiryTrackedItem(itemId);

        var earlierBatch = CreateBatch(itemId, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)), "EARLY001");
        var laterBatch = CreateBatch(itemId, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), "LATER001");

        _repository.GetByIdAsync(itemId, Arg.Any<CancellationToken>()).Returns(item);
        _repository.GetBatchesAsync(itemId, Arg.Any<CancellationToken>())
            .Returns([earlierBatch, laterBatch]); // Already in FEFO order

        var message = CreateEvent([
            new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(itemId, 7)
        ]);

        // Act
        await DeductTreatmentConsumablesHandler.Handle(
            message, _repository, _unitOfWork, CancellationToken.None);

        // Assert: FEFO = earlier batch deducted first
        earlierBatch.CurrentQuantity.Should().Be(3); // 10 - 7
        laterBatch.CurrentQuantity.Should().Be(10); // untouched
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Missing Item Tests

    [Fact]
    public async Task Handle_ConsumableNotFound_SkipsAndContinues()
    {
        // Arrange: one item exists, one does not
        var existingId = Guid.NewGuid();
        var missingId = Guid.NewGuid();
        var existingItem = CreateSimpleStockItem(existingId, currentStock: 10);

        _repository.GetByIdAsync(existingId, Arg.Any<CancellationToken>()).Returns(existingItem);
        _repository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((ConsumableItem?)null);

        var message = CreateEvent([
            new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(missingId, 3),
            new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(existingId, 2)
        ]);

        // Act — should NOT throw
        await DeductTreatmentConsumablesHandler.Handle(
            message, _repository, _unitOfWork, CancellationToken.None);

        // Assert: existing item was still deducted despite missing item
        existingItem.CurrentStock.Should().Be(8); // 10 - 2
        _repository.Received(1).Update(existingItem);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Empty Consumables Tests

    [Fact]
    public async Task Handle_EmptyConsumables_NoOp()
    {
        // Arrange
        var message = CreateEvent([]); // Empty list

        // Act
        await DeductTreatmentConsumablesHandler.Handle(
            message, _repository, _unitOfWork, CancellationToken.None);

        // Assert: no repository calls, no save
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Multiple Consumables Tests

    [Fact]
    public async Task Handle_MultipleConsumables_DeductsAll()
    {
        // Arrange
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();
        var item1 = CreateSimpleStockItem(item1Id, currentStock: 20);
        var item2 = CreateSimpleStockItem(item2Id, currentStock: 15);

        _repository.GetByIdAsync(item1Id, Arg.Any<CancellationToken>()).Returns(item1);
        _repository.GetByIdAsync(item2Id, Arg.Any<CancellationToken>()).Returns(item2);

        var message = CreateEvent([
            new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(item1Id, 5),
            new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(item2Id, 10)
        ]);

        // Act
        await DeductTreatmentConsumablesHandler.Handle(
            message, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        item1.CurrentStock.Should().Be(15); // 20 - 5
        item2.CurrentStock.Should().Be(5);  // 15 - 10
        _repository.Received(1).Update(item1);
        _repository.Received(1).Update(item2);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Insufficient Stock Tests

    [Fact]
    public async Task Handle_InsufficientSimpleStock_DeductsAvailable()
    {
        // Arrange: only 3 available but 10 requested
        var itemId = Guid.NewGuid();
        var item = CreateSimpleStockItem(itemId, currentStock: 3);

        _repository.GetByIdAsync(itemId, Arg.Any<CancellationToken>()).Returns(item);

        var message = CreateEvent([
            new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(itemId, 10)
        ]);

        // Act — should NOT throw, deduct what's available
        await DeductTreatmentConsumablesHandler.Handle(
            message, _repository, _unitOfWork, CancellationToken.None);

        // Assert: deducted all available (3), not the full 10
        item.CurrentStock.Should().Be(0);
        _repository.Received(1).Update(item);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InsufficientExpiryTrackedStock_DeductsAvailable()
    {
        // Arrange: batch has only 5 but 10 requested
        var itemId = Guid.NewGuid();
        var item = CreateExpiryTrackedItem(itemId);
        var batch = CreateBatch(itemId, 5, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), "BATCH001");

        _repository.GetByIdAsync(itemId, Arg.Any<CancellationToken>()).Returns(item);
        _repository.GetBatchesAsync(itemId, Arg.Any<CancellationToken>()).Returns([batch]);

        var message = CreateEvent([
            new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(itemId, 10)
        ]);

        // Act — should NOT throw, deduct what's available
        await DeductTreatmentConsumablesHandler.Handle(
            message, _repository, _unitOfWork, CancellationToken.None);

        // Assert: deducted all available from batch
        batch.CurrentQuantity.Should().Be(0); // Only had 5, deducted all 5
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}
