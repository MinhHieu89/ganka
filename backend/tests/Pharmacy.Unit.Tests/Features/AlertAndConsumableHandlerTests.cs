using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Pharmacy.Application.Features.Alerts;
using Pharmacy.Application.Features.Consumables;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for alert handlers (GetExpiryAlerts, GetLowStockAlerts) and
/// consumable item CRUD handlers (CreateConsumableItem, UpdateConsumableItem).
/// Follows the Wolverine static handler pattern established in SupplierHandlerTests.
/// </summary>
public class AlertAndConsumableHandlerTests
{
    private readonly IDrugBatchRepository _drugBatchRepository = Substitute.For<IDrugBatchRepository>();
    private readonly IConsumableRepository _consumableRepository = Substitute.For<IConsumableRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateConsumableItemCommand> _createValidator = Substitute.For<IValidator<CreateConsumableItemCommand>>();
    private readonly IValidator<UpdateConsumableItemCommand> _updateValidator = Substitute.For<IValidator<UpdateConsumableItemCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultItemId = Guid.NewGuid();

    public AlertAndConsumableHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    private void SetupValidCreateValidator()
    {
        _createValidator.ValidateAsync(Arg.Any<CreateConsumableItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidUpdateValidator()
    {
        _updateValidator.ValidateAsync(Arg.Any<UpdateConsumableItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static CreateConsumableItemCommand CreateValidCreateCommand() =>
        new(
            Name: "IPL Gel",
            NameVi: "Gel IPL",
            Unit: "Tube",
            TrackingMode: (int)ConsumableTrackingMode.SimpleStock,
            MinStockLevel: 5);

    private static UpdateConsumableItemCommand CreateValidUpdateCommand(Guid? id = null) =>
        new(
            Id: id ?? DefaultItemId,
            Name: "IPL Gel (Updated)",
            NameVi: "Gel IPL (Cập nhật)",
            Unit: "Box",
            TrackingMode: (int)ConsumableTrackingMode.ExpiryTracked,
            MinStockLevel: 10);

    #region GetExpiryAlerts Tests

    [Fact]
    public async Task GetExpiryAlerts_ReturnsNearExpiryBatches()
    {
        // Arrange
        var expectedAlerts = new List<ExpiryAlertDto>
        {
            new(Guid.NewGuid(), "Amoxicillin", "BATCH001", DateOnly.FromDateTime(DateTime.Today.AddDays(30)), 50, 30),
            new(Guid.NewGuid(), "Cetirizine", "BATCH002", DateOnly.FromDateTime(DateTime.Today.AddDays(60)), 20, 60)
        };

        _drugBatchRepository.GetExpiryAlertsAsync(90, Arg.Any<CancellationToken>())
            .Returns(expectedAlerts);

        var query = new GetExpiryAlertsQuery();

        // Act
        var result = await GetExpiryAlertsHandler.Handle(query, _drugBatchRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].BatchNumber.Should().Be("BATCH001");
        result[1].BatchNumber.Should().Be("BATCH002");
        await _drugBatchRepository.Received(1).GetExpiryAlertsAsync(90, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetExpiryAlerts_ExcludesExpiredAndEmpty()
    {
        // Arrange: repository returns only non-expired, non-empty batches (filtering is done in repository)
        var expectedAlerts = new List<ExpiryAlertDto>
        {
            new(Guid.NewGuid(), "Vitamin C", "BATCH003", DateOnly.FromDateTime(DateTime.Today.AddDays(45)), 10, 45)
        };

        _drugBatchRepository.GetExpiryAlertsAsync(90, Arg.Any<CancellationToken>())
            .Returns(expectedAlerts);

        var query = new GetExpiryAlertsQuery();

        // Act
        var result = await GetExpiryAlertsHandler.Handle(query, _drugBatchRepository, CancellationToken.None);

        // Assert -- handler delegates filtering to repository; result only has the valid batch
        result.Should().HaveCount(1);
        result[0].DaysUntilExpiry.Should().Be(45);
        result[0].CurrentQuantity.Should().BePositive();
    }

    [Fact]
    public async Task GetExpiryAlerts_ConfigurableThreshold()
    {
        // Arrange
        var alerts30Days = new List<ExpiryAlertDto>
        {
            new(Guid.NewGuid(), "Drug A", "BA001", DateOnly.FromDateTime(DateTime.Today.AddDays(25)), 5, 25)
        };
        var alerts60Days = new List<ExpiryAlertDto>
        {
            new(Guid.NewGuid(), "Drug A", "BA001", DateOnly.FromDateTime(DateTime.Today.AddDays(25)), 5, 25),
            new(Guid.NewGuid(), "Drug B", "BB001", DateOnly.FromDateTime(DateTime.Today.AddDays(55)), 8, 55)
        };

        _drugBatchRepository.GetExpiryAlertsAsync(30, Arg.Any<CancellationToken>()).Returns(alerts30Days);
        _drugBatchRepository.GetExpiryAlertsAsync(60, Arg.Any<CancellationToken>()).Returns(alerts60Days);

        var query30 = new GetExpiryAlertsQuery(30);
        var query60 = new GetExpiryAlertsQuery(60);

        // Act
        var result30 = await GetExpiryAlertsHandler.Handle(query30, _drugBatchRepository, CancellationToken.None);
        var result60 = await GetExpiryAlertsHandler.Handle(query60, _drugBatchRepository, CancellationToken.None);

        // Assert
        result30.Should().HaveCount(1);
        result60.Should().HaveCount(2);
        await _drugBatchRepository.Received(1).GetExpiryAlertsAsync(30, Arg.Any<CancellationToken>());
        await _drugBatchRepository.Received(1).GetExpiryAlertsAsync(60, Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetLowStockAlerts Tests

    [Fact]
    public async Task GetLowStockAlerts_ReturnsBelowMinimum()
    {
        // Arrange
        var expectedAlerts = new List<LowStockAlertDto>
        {
            new(Guid.NewGuid(), "Amoxicillin 500mg", 3, 10),
            new(Guid.NewGuid(), "Cetirizine 10mg", 0, 5)
        };

        _drugBatchRepository.GetLowStockAlertsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedAlerts);

        var query = new GetLowStockAlertsQuery();

        // Act
        var result = await GetLowStockAlertsHandler.Handle(query, _drugBatchRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.TotalStock.Should().BeLessThan(dto.MinStockLevel));
        await _drugBatchRepository.Received(1).GetLowStockAlertsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLowStockAlerts_ExcludesZeroMinLevel()
    {
        // Arrange: repository excludes drugs with MinStockLevel = 0 (filtering done in repository)
        var expectedAlerts = new List<LowStockAlertDto>
        {
            new(Guid.NewGuid(), "Drug With Min Level", 2, 10)
        };

        _drugBatchRepository.GetLowStockAlertsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedAlerts);

        var query = new GetLowStockAlertsQuery();

        // Act
        var result = await GetLowStockAlertsHandler.Handle(query, _drugBatchRepository, CancellationToken.None);

        // Assert -- only drugs with MinStockLevel > 0 are returned by repository
        result.Should().HaveCount(1);
        result[0].MinStockLevel.Should().BePositive();
    }

    #endregion

    #region CreateConsumableItem Tests

    [Fact]
    public async Task CreateConsumableItem_Valid_CreatesItem()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = CreateValidCreateCommand();

        // Act
        var result = await CreateConsumableItemHandler.Handle(
            command, _consumableRepository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _consumableRepository.Received(1).Add(Arg.Any<ConsumableItem>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateConsumableItem_EmptyName_ValidationError()
    {
        // Arrange
        var command = CreateValidCreateCommand() with { Name = "" };
        _createValidator.ValidateAsync(Arg.Any<CreateConsumableItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Consumable item name is required.")
            }));

        // Act
        var result = await CreateConsumableItemHandler.Handle(
            command, _consumableRepository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        _consumableRepository.DidNotReceive().Add(Arg.Any<ConsumableItem>());
    }

    #endregion

    #region UpdateConsumableItem Tests

    [Fact]
    public async Task UpdateConsumableItem_Existing_UpdatesFields()
    {
        // Arrange
        SetupValidUpdateValidator();
        var itemId = Guid.NewGuid();
        var existingItem = ConsumableItem.Create(
            "IPL Gel",
            "Gel IPL",
            "Tube",
            ConsumableTrackingMode.SimpleStock,
            5,
            new BranchId(DefaultBranchId));

        _consumableRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(existingItem);

        var command = CreateValidUpdateCommand(itemId);

        // Act
        var result = await UpdateConsumableItemHandler.Handle(
            command, _consumableRepository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        existingItem.Name.Should().Be(command.Name);
        existingItem.NameVi.Should().Be(command.NameVi);
        existingItem.Unit.Should().Be(command.Unit);
        existingItem.MinStockLevel.Should().Be(command.MinStockLevel);
    }

    [Fact]
    public async Task UpdateConsumableItem_NotFound_ReturnsError()
    {
        // Arrange
        SetupValidUpdateValidator();
        var nonExistentId = Guid.NewGuid();
        _consumableRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((ConsumableItem?)null);

        var command = CreateValidUpdateCommand(nonExistentId);

        // Act
        var result = await UpdateConsumableItemHandler.Handle(
            command, _consumableRepository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}
