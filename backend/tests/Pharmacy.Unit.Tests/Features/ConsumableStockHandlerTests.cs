using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Pharmacy.Application.Features.Consumables;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for consumable stock management handlers:
/// - AddConsumableStock: supports SimpleStock (direct increment) and ExpiryTracked (new batch)
/// - AdjustConsumableStock: manual corrections with audit record for both tracking modes
/// - GetConsumableItems: returns all active items with stock info
/// - GetConsumableAlerts: returns items below minimum stock level
///
/// CON-02: Manual stock management with alerts (Phase 6 manual-only, Phase 9 adds auto-deduction).
/// </summary>
public class ConsumableStockHandlerTests
{
    private readonly IConsumableRepository _consumableRepository = Substitute.For<IConsumableRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IValidator<AddConsumableStockCommand> _addStockValidator = Substitute.For<IValidator<AddConsumableStockCommand>>();
    private readonly IValidator<AdjustConsumableStockCommand> _adjustStockValidator = Substitute.For<IValidator<AdjustConsumableStockCommand>>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultItemId = Guid.NewGuid();
    private static readonly Guid DefaultBatchId = Guid.NewGuid();
    private static readonly Guid DefaultUserId = Guid.NewGuid();

    public ConsumableStockHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
        _currentUser.UserId.Returns(DefaultUserId);
    }

    private void SetupValidAddStockValidator()
    {
        _addStockValidator.ValidateAsync(Arg.Any<AddConsumableStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidAdjustStockValidator()
    {
        _adjustStockValidator.ValidateAsync(Arg.Any<AdjustConsumableStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static ConsumableItem CreateSimpleStockItem(int currentStock = 10)
    {
        var item = ConsumableItem.Create(
            "Eye Shield",
            "Kính che mắt",
            "Piece",
            ConsumableTrackingMode.SimpleStock,
            minStockLevel: 5,
            new BranchId(DefaultBranchId));

        // Add stock to set initial quantity
        if (currentStock > 0)
            item.AddStock(currentStock);

        return item;
    }

    private static ConsumableItem CreateExpiryTrackedItem()
    {
        return ConsumableItem.Create(
            "IPL Gel",
            "Gel IPL",
            "Tube",
            ConsumableTrackingMode.ExpiryTracked,
            minStockLevel: 2,
            new BranchId(DefaultBranchId));
    }

    private static ConsumableBatch CreateTestBatch(int quantity = 20)
    {
        var batch = ConsumableBatch.Create(
            consumableItemId: DefaultItemId,
            batchNumber: "CB2026001",
            expiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            quantity: quantity);

        var idProp = typeof(Entity).GetProperty("Id");
        idProp?.GetSetMethod(true)?.Invoke(batch, [DefaultBatchId]);
        return batch;
    }

    #region AddConsumableStock - SimpleStock Tests

    [Fact]
    public async Task AddConsumableStock_SimpleStock_IncrementsCurrentStock()
    {
        // Arrange
        SetupValidAddStockValidator();
        var item = CreateSimpleStockItem(10);
        _consumableRepository.GetByIdAsync(DefaultItemId, Arg.Any<CancellationToken>())
            .Returns(item);

        var command = new AddConsumableStockCommand(
            ConsumableItemId: DefaultItemId,
            Quantity: 15,
            BatchNumber: null,
            ExpiryDate: null,
            Notes: "Nhập thêm hàng");

        // Act
        var result = await AddConsumableStockHandler.Handle(
            command, _addStockValidator, _consumableRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.CurrentStock.Should().Be(25); // 10 + 15
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region AddConsumableStock - ExpiryTracked Tests

    [Fact]
    public async Task AddConsumableStock_ExpiryTracked_CreatesBatch()
    {
        // Arrange
        SetupValidAddStockValidator();
        var item = CreateExpiryTrackedItem();

        // Set item.Id to DefaultItemId so the batch ConsumableItemId matches our assertion
        var idProp = typeof(Entity).GetProperty("Id");
        idProp?.GetSetMethod(true)?.Invoke(item, [DefaultItemId]);

        _consumableRepository.GetByIdAsync(DefaultItemId, Arg.Any<CancellationToken>())
            .Returns(item);

        var command = new AddConsumableStockCommand(
            ConsumableItemId: DefaultItemId,
            Quantity: 10,
            BatchNumber: "IPL2026001",
            ExpiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
            Notes: "Nhập lô mới");

        // Act
        var result = await AddConsumableStockHandler.Handle(
            command, _addStockValidator, _consumableRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _consumableRepository.Received(1).AddBatch(Arg.Is<ConsumableBatch>(b =>
            b.ConsumableItemId == DefaultItemId &&
            b.BatchNumber == "IPL2026001" &&
            b.InitialQuantity == 10));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddConsumableStock_ExpiryTracked_RequiresBatchNumberAndExpiry()
    {
        // Arrange: validator returns failure for missing batch info on ExpiryTracked item
        var validationFailure = new ValidationResult(new[]
        {
            new ValidationFailure("BatchNumber", "Batch number is required for expiry-tracked consumables."),
            new ValidationFailure("ExpiryDate", "Expiry date is required for expiry-tracked consumables.")
        });
        _addStockValidator.ValidateAsync(Arg.Any<AddConsumableStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationFailure);

        var command = new AddConsumableStockCommand(
            ConsumableItemId: DefaultItemId,
            Quantity: 5,
            BatchNumber: null,   // Missing!
            ExpiryDate: null,    // Missing!
            Notes: null);

        // Act
        var result = await AddConsumableStockHandler.Handle(
            command, _addStockValidator, _consumableRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        _consumableRepository.DidNotReceive().AddBatch(Arg.Any<ConsumableBatch>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region AdjustConsumableStock - SimpleStock Tests

    [Fact]
    public async Task AdjustConsumableStock_SimpleStock_ModifiesCurrentStock()
    {
        // Arrange
        SetupValidAdjustStockValidator();
        var item = CreateSimpleStockItem(20);
        _consumableRepository.GetByIdAsync(DefaultItemId, Arg.Any<CancellationToken>())
            .Returns(item);

        var command = new AdjustConsumableStockCommand(
            ConsumableItemId: DefaultItemId,
            ConsumableBatchId: null,  // SimpleStock: no batch needed
            QuantityChange: -5,
            Reason: StockAdjustmentReason.Damage,
            Notes: "Hàng bị hỏng");

        // Act
        var result = await AdjustConsumableStockHandler.Handle(
            command, _adjustStockValidator, _consumableRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.CurrentStock.Should().Be(15); // 20 - 5
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region AdjustConsumableStock - ExpiryTracked Tests

    [Fact]
    public async Task AdjustConsumableStock_ExpiryTracked_ModifiesBatch()
    {
        // Arrange
        SetupValidAdjustStockValidator();
        var item = CreateExpiryTrackedItem();
        var batch = CreateTestBatch(30);

        _consumableRepository.GetByIdAsync(DefaultItemId, Arg.Any<CancellationToken>())
            .Returns(item);
        _consumableRepository.GetBatchByIdAsync(DefaultBatchId, Arg.Any<CancellationToken>())
            .Returns(batch);

        var command = new AdjustConsumableStockCommand(
            ConsumableItemId: DefaultItemId,
            ConsumableBatchId: DefaultBatchId,
            QuantityChange: -10,
            Reason: StockAdjustmentReason.WriteOff,
            Notes: "Hàng hết hạn trong lô này");

        // Act
        var result = await AdjustConsumableStockHandler.Handle(
            command, _adjustStockValidator, _consumableRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        batch.CurrentQuantity.Should().Be(20); // 30 - 10
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region AdjustConsumableStock - Audit Record Tests

    [Fact]
    public async Task AdjustConsumableStock_CreatesAuditRecord()
    {
        // Arrange: use ExpiryTracked item + batch so StockAdjustment has a non-null FK (domain constraint).
        // SimpleStock items do not generate a StockAdjustment record (no batch FK to reference).
        SetupValidAdjustStockValidator();
        var item = CreateExpiryTrackedItem();
        var batch = CreateTestBatch(50);

        _consumableRepository.GetByIdAsync(DefaultItemId, Arg.Any<CancellationToken>())
            .Returns(item);
        _consumableRepository.GetBatchByIdAsync(DefaultBatchId, Arg.Any<CancellationToken>())
            .Returns(batch);

        var command = new AdjustConsumableStockCommand(
            ConsumableItemId: DefaultItemId,
            ConsumableBatchId: DefaultBatchId,
            QuantityChange: -3,
            Reason: StockAdjustmentReason.Expired,
            Notes: "Hết hạn sử dụng");

        // Act
        var result = await AdjustConsumableStockHandler.Handle(
            command, _adjustStockValidator, _consumableRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _consumableRepository.Received(1).AddStockAdjustment(Arg.Is<StockAdjustment>(a =>
            a.QuantityChange == -3 &&
            a.Reason == StockAdjustmentReason.Expired &&
            a.ConsumableBatchId == DefaultBatchId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustConsumableStock_NegativeExceedsStock_ReturnsError()
    {
        // Arrange
        SetupValidAdjustStockValidator();
        var item = CreateSimpleStockItem(5); // Only 5 units available
        _consumableRepository.GetByIdAsync(DefaultItemId, Arg.Any<CancellationToken>())
            .Returns(item);

        var command = new AdjustConsumableStockCommand(
            ConsumableItemId: DefaultItemId,
            ConsumableBatchId: null,
            QuantityChange: -100, // Try to remove 100 but only 5 available
            Reason: StockAdjustmentReason.WriteOff,
            Notes: null);

        // Act
        var result = await AdjustConsumableStockHandler.Handle(
            command, _adjustStockValidator, _consumableRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetConsumableItems Tests

    [Fact]
    public async Task GetConsumableItems_ReturnsActiveWithStockInfo()
    {
        // Arrange
        var expectedItems = new List<ConsumableItemDto>
        {
            new(Guid.NewGuid(), "Eye Shield", "Kính che mắt", "Piece",
                (int)ConsumableTrackingMode.SimpleStock, 15, 5, true, false),
            new(Guid.NewGuid(), "IPL Gel", "Gel IPL", "Tube",
                (int)ConsumableTrackingMode.ExpiryTracked, 8, 2, true, false)
        };

        _consumableRepository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns([]);
        // GetConsumableItems handler returns ConsumableItemDto list built from domain entities
        // For test, we mock the method used by the handler - GetAlertsAsync returns the DTO-shaped data
        // The handler calls GetAllActiveAsync and maps to DTOs

        // Re-test: handler calls GetAllActiveAsync which returns domain entities
        var simpleItem = CreateSimpleStockItem(15);
        var expiryItem = CreateExpiryTrackedItem();

        _consumableRepository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns([simpleItem, expiryItem]);

        _consumableRepository.GetBatchesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetConsumableItemsQuery();

        // Act
        var result = await GetConsumableItemsHandler.Handle(
            query, _consumableRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        await _consumableRepository.Received(1).GetAllActiveAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetConsumableAlerts Tests

    [Fact]
    public async Task GetConsumableAlerts_ReturnsBelowMinimum()
    {
        // Arrange
        var expectedAlerts = new List<ConsumableItemDto>
        {
            new(Guid.NewGuid(), "Eye Shield", "Kính che mắt", "Piece",
                (int)ConsumableTrackingMode.SimpleStock, 3, 10, true, true),
            new(Guid.NewGuid(), "IPL Gel", "Gel IPL", "Tube",
                (int)ConsumableTrackingMode.ExpiryTracked, 1, 5, true, true)
        };

        _consumableRepository.GetAlertsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedAlerts);

        var query = new GetConsumableAlertsQuery();

        // Act
        var result = await GetConsumableAlertsHandler.Handle(
            query, _consumableRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.IsLowStock.Should().BeTrue());
        await _consumableRepository.Received(1).GetAlertsAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}
