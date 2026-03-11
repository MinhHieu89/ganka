using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Pharmacy.Application.Features.Inventory;
using Pharmacy.Application.Features.OtcSales;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Services;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for OTC sale and inventory management handlers.
/// PHR-06: Walk-in OTC sales use same FEFO mechanism as dispensing.
/// PHR-01: Inventory handlers provide drug stock views, batch detail queries, and manual adjustments.
/// </summary>
public class OtcSaleAndInventoryHandlerTests
{
    private readonly IOtcSaleRepository _otcSaleRepository = Substitute.For<IOtcSaleRepository>();
    private readonly IDrugBatchRepository _drugBatchRepository = Substitute.For<IDrugBatchRepository>();
    private readonly IDrugCatalogItemRepository _drugCatalogItemRepository = Substitute.For<IDrugCatalogItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IValidator<CreateOtcSaleCommand> _createOtcSaleValidator = Substitute.For<IValidator<CreateOtcSaleCommand>>();
    private readonly IValidator<AdjustStockCommand> _adjustStockValidator = Substitute.For<IValidator<AdjustStockCommand>>();
    private readonly ISupplierRepository _supplierRepository = Substitute.For<ISupplierRepository>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultPatientId = Guid.NewGuid();
    private static readonly Guid DefaultDrugId = Guid.NewGuid();
    private static readonly Guid DefaultBatchId = Guid.NewGuid();

    public OtcSaleAndInventoryHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    private void SetupValidOtcSaleValidator()
    {
        _createOtcSaleValidator.ValidateAsync(Arg.Any<CreateOtcSaleCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidAdjustStockValidator()
    {
        _adjustStockValidator.ValidateAsync(Arg.Any<AdjustStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    /// <summary>
    /// Creates a test DrugBatch with a known batch ID and specified quantity.
    /// Uses reflection to set the Id since EF-style private setters require it.
    /// </summary>
    private static DrugBatch CreateTestBatch(int quantity = 100)
    {
        var batch = DrugBatch.Create(
            drugCatalogItemId: DefaultDrugId,
            supplierId: Guid.NewGuid(),
            batchNumber: "BN2026001",
            expiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            quantity: quantity,
            purchasePrice: 50_000m);

        var idProp = typeof(Entity).GetProperty("Id");
        idProp?.GetSetMethod(true)?.Invoke(batch, [DefaultBatchId]);
        return batch;
    }

    #region CreateOtcSale Tests

    [Fact]
    public async Task CreateOtcSale_ValidWithCustomer_CreatesSaleAndDeductsStock()
    {
        // Arrange
        SetupValidOtcSaleValidator();
        var batch = CreateTestBatch(100);

        _drugBatchRepository.GetAvailableBatchesFEFOAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns([batch]);
        _drugBatchRepository.GetByIdAsync(DefaultBatchId, Arg.Any<CancellationToken>())
            .Returns(batch);

        var command = new CreateOtcSaleCommand(
            PatientId: DefaultPatientId,
            CustomerName: "Nguyễn Văn An",
            Notes: null,
            Lines:
            [
                new OtcSaleLineInput(DefaultDrugId, "Tobramycin 0.3%", 5, 150_000m)
            ]);

        // Act
        var result = await CreateOtcSaleHandler.Handle(
            command, _createOtcSaleValidator, _otcSaleRepository, _drugBatchRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _otcSaleRepository.Received(1).Add(Arg.Is<OtcSale>(s =>
            s.PatientId == DefaultPatientId &&
            s.CustomerName == "Nguyễn Văn An"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOtcSale_Anonymous_CreatesSale()
    {
        // Arrange
        SetupValidOtcSaleValidator();
        var batch = CreateTestBatch(50);

        _drugBatchRepository.GetAvailableBatchesFEFOAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns([batch]);
        _drugBatchRepository.GetByIdAsync(DefaultBatchId, Arg.Any<CancellationToken>())
            .Returns(batch);

        var command = new CreateOtcSaleCommand(
            PatientId: null,     // Anonymous: no patient link
            CustomerName: null,  // Anonymous: no name
            Notes: "Khách lẻ",
            Lines:
            [
                new OtcSaleLineInput(DefaultDrugId, "Vitamin C 500mg", 10, 20_000m)
            ]);

        // Act
        var result = await CreateOtcSaleHandler.Handle(
            command, _createOtcSaleValidator, _otcSaleRepository, _drugBatchRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _otcSaleRepository.Received(1).Add(Arg.Is<OtcSale>(s =>
            s.PatientId == null && s.CustomerName == null));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOtcSale_InsufficientStock_ReturnsError()
    {
        // Arrange
        SetupValidOtcSaleValidator();
        var batch = CreateTestBatch(3); // Only 3 units available

        _drugBatchRepository.GetAvailableBatchesFEFOAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns([batch]);

        var command = new CreateOtcSaleCommand(
            PatientId: null,
            CustomerName: null,
            Notes: null,
            Lines:
            [
                new OtcSaleLineInput(DefaultDrugId, "Tobramycin 0.3%", 100, 150_000m) // Need 100 but only 3 available
            ]);

        // Act
        var result = await CreateOtcSaleHandler.Handle(
            command, _createOtcSaleValidator, _otcSaleRepository, _drugBatchRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Tobramycin 0.3%");
        _otcSaleRepository.DidNotReceive().Add(Arg.Any<OtcSale>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region AdjustStock Tests

    [Fact]
    public async Task AdjustStock_PositiveAdjustment_IncreasesQuantity()
    {
        // Arrange
        SetupValidAdjustStockValidator();
        var batch = CreateTestBatch(20);

        _drugBatchRepository.GetByIdAsync(DefaultBatchId, Arg.Any<CancellationToken>())
            .Returns(batch);

        var command = new AdjustStockCommand(
            DrugBatchId: DefaultBatchId,
            QuantityChange: 10,  // Add 10 units
            Reason: StockAdjustmentReason.Correction,
            Notes: "Kiểm kê lại");

        // Act
        var result = await AdjustStockHandler.Handle(
            command, _adjustStockValidator, _drugBatchRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        batch.CurrentQuantity.Should().Be(30); // 20 + 10
    }

    [Fact]
    public async Task AdjustStock_NegativeAdjustment_DecreasesQuantity()
    {
        // Arrange
        SetupValidAdjustStockValidator();
        var batch = CreateTestBatch(20);

        _drugBatchRepository.GetByIdAsync(DefaultBatchId, Arg.Any<CancellationToken>())
            .Returns(batch);

        var command = new AdjustStockCommand(
            DrugBatchId: DefaultBatchId,
            QuantityChange: -5,  // Remove 5 units
            Reason: StockAdjustmentReason.Damage,
            Notes: "Thuốc bị vỡ");

        // Act
        var result = await AdjustStockHandler.Handle(
            command, _adjustStockValidator, _drugBatchRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        batch.CurrentQuantity.Should().Be(15); // 20 - 5
    }

    [Fact]
    public async Task AdjustStock_ExcessiveNegative_ReturnsError()
    {
        // Arrange
        SetupValidAdjustStockValidator();
        var batch = CreateTestBatch(5); // Only 5 units available

        _drugBatchRepository.GetByIdAsync(DefaultBatchId, Arg.Any<CancellationToken>())
            .Returns(batch);

        var command = new AdjustStockCommand(
            DrugBatchId: DefaultBatchId,
            QuantityChange: -100, // Try to remove 100 but only 5 available
            Reason: StockAdjustmentReason.WriteOff,
            Notes: null);

        // Act
        var result = await AdjustStockHandler.Handle(
            command, _adjustStockValidator, _drugBatchRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustStock_CreatesAuditRecord()
    {
        // Arrange
        SetupValidAdjustStockValidator();
        var batch = CreateTestBatch(50);

        _drugBatchRepository.GetByIdAsync(DefaultBatchId, Arg.Any<CancellationToken>())
            .Returns(batch);

        var command = new AdjustStockCommand(
            DrugBatchId: DefaultBatchId,
            QuantityChange: -3,
            Reason: StockAdjustmentReason.Expired,
            Notes: "Hàng hết hạn");

        // Act
        var result = await AdjustStockHandler.Handle(
            command, _adjustStockValidator, _drugBatchRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _drugBatchRepository.Received(1).AddStockAdjustment(Arg.Is<StockAdjustment>(a =>
            a.DrugBatchId == DefaultBatchId &&
            a.QuantityChange == -3 &&
            a.Reason == StockAdjustmentReason.Expired));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetDrugInventory Tests

    [Fact]
    public async Task GetDrugInventory_ReturnsComputedStockLevels()
    {
        // Arrange
        var inventoryDtos = new List<DrugInventoryDto>
        {
            new(
                DrugCatalogItemId: DefaultDrugId,
                Name: "Tobramycin 0.3%",
                NameVi: "Tobramycin 0.3%",
                GenericName: "Tobramycin",
                Unit: "Chai",
                Form: (int)DrugForm.EyeDrops,
                Route: (int)DrugRoute.Topical,
                SellingPrice: 150_000m,
                MinStockLevel: 10,
                TotalStock: 45,
                BatchCount: 2,
                IsLowStock: false,
                HasExpiryAlert: false)
        };

        _drugCatalogItemRepository.GetAllWithInventoryAsync(30, Arg.Any<CancellationToken>())
            .Returns(inventoryDtos);

        var query = new GetDrugInventoryQuery(ExpiryAlertDays: 30);

        // Act
        var result = await GetDrugInventoryHandler.Handle(
            query, _drugCatalogItemRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].DrugCatalogItemId.Should().Be(DefaultDrugId);
        result.Value[0].TotalStock.Should().Be(45);
        result.Value[0].IsLowStock.Should().BeFalse();
    }

    #endregion

    #region GetDrugBatches Tests

    [Fact]
    public async Task GetDrugBatches_ValidDrug_ReturnsBatchesFEFOOrdered()
    {
        // Arrange
        // Create two batches with different expiry dates
        var earlierBatch = DrugBatch.Create(DefaultDrugId, Guid.NewGuid(), "BN001",
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)), 10, 50_000m);
        var laterBatch = DrugBatch.Create(DefaultDrugId, Guid.NewGuid(), "BN002",
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), 20, 55_000m);

        // IDrugBatchRepository.GetBatchesForDrugAsync returns DrugBatch domain entities
        // The handler maps them to DrugBatchDto for the response
        _drugBatchRepository.GetBatchesForDrugAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns([earlierBatch, laterBatch]);

        var query = new GetDrugBatchesQuery(DrugCatalogItemId: DefaultDrugId);

        // Act
        var result = await GetDrugBatchesHandler.Handle(
            query, _drugBatchRepository, _supplierRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        // First item should be the earliest-expiring batch (FEFO order)
        result.Value[0].BatchNumber.Should().Be("BN001");
        result.Value[1].BatchNumber.Should().Be("BN002");
    }

    [Fact]
    public async Task GetDrugBatches_NoMatchingDrug_ReturnsEmptyList()
    {
        // Arrange
        var unknownDrugId = Guid.NewGuid();
        _drugBatchRepository.GetBatchesForDrugAsync(unknownDrugId, Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetDrugBatchesQuery(DrugCatalogItemId: unknownDrugId);

        // Act
        var result = await GetDrugBatchesHandler.Handle(
            query, _drugBatchRepository, _supplierRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion
}
