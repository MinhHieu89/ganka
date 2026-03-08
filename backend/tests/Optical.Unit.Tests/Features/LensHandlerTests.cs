using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Optical.Application.Features.Lenses;
using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// Unit tests for Lens Catalog handlers:
///   - CreateLensCatalogItemHandler (OPT-02)
///   - UpdateLensCatalogItemHandler (OPT-02)
///   - GetLensCatalogHandler (OPT-02)
///   - AdjustLensStockHandler (OPT-02)
/// Follows the NSubstitute + FluentAssertions pattern established in project tests.
/// </summary>
public class LensHandlerTests
{
    // ─── Shared Test Infrastructure ───────────────────────────────────────────

    private static readonly BranchId TestBranchId = new(Guid.NewGuid());

    private static ICurrentUser CreateCurrentUser(Guid? branchId = null)
    {
        var user = Substitute.For<ICurrentUser>();
        user.BranchId.Returns((branchId ?? TestBranchId.Value));
        user.UserId.Returns(Guid.NewGuid());
        return user;
    }

    private static LensCatalogItem CreateCatalogItem(
        string brand = "Essilor",
        string name = "Crizal Single Vision",
        string lensType = "single_vision",
        LensMaterial material = LensMaterial.CR39,
        LensCoating coatings = LensCoating.AntiReflective,
        decimal sellingPrice = 1_200_000m,
        decimal costPrice = 600_000m,
        Guid? supplierId = null)
        => LensCatalogItem.Create(brand, name, lensType, material, coatings, sellingPrice, costPrice, supplierId, TestBranchId);

    // ─── CreateLensCatalogItem Tests ──────────────────────────────────────────

    [Fact]
    public async Task CreateLensCatalogItem_ValidCommand_ReturnsGuid()
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new CreateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new CreateLensCatalogItemCommand(
            Brand: "Essilor",
            Name: "Crizal Single Vision",
            LensType: "single_vision",
            Material: (int)LensMaterial.CR39,
            AvailableCoatings: (int)LensCoating.AntiReflective,
            SellingPrice: 1_200_000m,
            CostPrice: 600_000m,
            PreferredSupplierId: null);

        // Act
        var result = await CreateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        repository.Received(1).Add(Arg.Any<LensCatalogItem>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("", "Crizal", "single_vision")]
    [InlineData("Essilor", "", "single_vision")]
    [InlineData("Essilor", "Crizal", "unknown_type")]
    public async Task CreateLensCatalogItem_InvalidCommand_ReturnsValidationError(
        string brand, string name, string lensType)
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new CreateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new CreateLensCatalogItemCommand(
            Brand: brand,
            Name: name,
            LensType: lensType,
            Material: (int)LensMaterial.CR39,
            AvailableCoatings: (int)LensCoating.None,
            SellingPrice: 1_000_000m,
            CostPrice: 500_000m,
            PreferredSupplierId: null);

        // Act
        var result = await CreateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        repository.DidNotReceive().Add(Arg.Any<LensCatalogItem>());
    }

    [Fact]
    public async Task CreateLensCatalogItem_NegativeSellingPrice_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new CreateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new CreateLensCatalogItemCommand(
            Brand: "Essilor",
            Name: "Crizal",
            LensType: "single_vision",
            Material: (int)LensMaterial.CR39,
            AvailableCoatings: (int)LensCoating.None,
            SellingPrice: -100m,
            CostPrice: 500_000m,
            PreferredSupplierId: null);

        // Act
        var result = await CreateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task CreateLensCatalogItem_NegativeCostPrice_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new CreateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new CreateLensCatalogItemCommand(
            Brand: "Essilor",
            Name: "Crizal",
            LensType: "single_vision",
            Material: (int)LensMaterial.CR39,
            AvailableCoatings: (int)LensCoating.None,
            SellingPrice: 1_000_000m,
            CostPrice: -1m,
            PreferredSupplierId: null);

        // Act
        var result = await CreateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Theory]
    [InlineData("single_vision")]
    [InlineData("bifocal")]
    [InlineData("progressive")]
    [InlineData("reading")]
    public async Task CreateLensCatalogItem_AllValidLensTypes_Succeed(string lensType)
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new CreateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new CreateLensCatalogItemCommand(
            Brand: "Hoya",
            Name: "Test Lens",
            LensType: lensType,
            Material: (int)LensMaterial.Polycarbonate,
            AvailableCoatings: (int)LensCoating.None,
            SellingPrice: 500_000m,
            CostPrice: 250_000m,
            PreferredSupplierId: null);

        // Act
        var result = await CreateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ─── UpdateLensCatalogItem Tests ──────────────────────────────────────────

    [Fact]
    public async Task UpdateLensCatalogItem_ValidCommand_UpdatesItem()
    {
        // Arrange
        var existingItem = CreateCatalogItem();
        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(existingItem.Id, Arg.Any<CancellationToken>())
            .Returns(existingItem);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new UpdateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new UpdateLensCatalogItemCommand(
            Id: existingItem.Id,
            Brand: "Hoya",
            Name: "Sensity Progressive",
            LensType: "progressive",
            Material: (int)LensMaterial.HiIndex,
            AvailableCoatings: (int)(LensCoating.AntiReflective | LensCoating.BlueCut),
            SellingPrice: 2_500_000m,
            CostPrice: 1_200_000m,
            PreferredSupplierId: null,
            IsActive: true);

        // Act
        var result = await UpdateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingItem.Brand.Should().Be("Hoya");
        existingItem.Name.Should().Be("Sensity Progressive");
        existingItem.LensType.Should().Be("progressive");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateLensCatalogItem_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((LensCatalogItem?)null);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new UpdateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new UpdateLensCatalogItemCommand(
            Id: Guid.NewGuid(),
            Brand: "Essilor",
            Name: "Varilux",
            LensType: "progressive",
            Material: (int)LensMaterial.HiIndex,
            AvailableCoatings: (int)LensCoating.None,
            SellingPrice: 3_000_000m,
            CostPrice: 1_500_000m,
            PreferredSupplierId: null,
            IsActive: true);

        // Act
        var result = await UpdateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateLensCatalogItem_InvalidCommand_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new UpdateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new UpdateLensCatalogItemCommand(
            Id: Guid.Empty,          // invalid
            Brand: "",               // invalid
            Name: "Valid Name",
            LensType: "single_vision",
            Material: (int)LensMaterial.CR39,
            AvailableCoatings: (int)LensCoating.None,
            SellingPrice: -1m,       // invalid
            CostPrice: 500_000m,
            PreferredSupplierId: null,
            IsActive: true);

        // Act
        var result = await UpdateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task UpdateLensCatalogItem_Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        var existingItem = CreateCatalogItem();
        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(existingItem.Id, Arg.Any<CancellationToken>())
            .Returns(existingItem);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = new UpdateLensCatalogItemCommandValidator();
        var currentUser = CreateCurrentUser();

        var command = new UpdateLensCatalogItemCommand(
            Id: existingItem.Id,
            Brand: "Essilor",
            Name: "Crizal Single Vision",
            LensType: "single_vision",
            Material: (int)LensMaterial.CR39,
            AvailableCoatings: (int)LensCoating.AntiReflective,
            SellingPrice: 1_200_000m,
            CostPrice: 600_000m,
            PreferredSupplierId: null,
            IsActive: false);  // deactivate

        // Act
        var result = await UpdateLensCatalogItemHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingItem.IsActive.Should().BeFalse();
    }

    // ─── GetLensCatalog Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task GetLensCatalog_ActiveOnly_ReturnsActiveItems()
    {
        // Arrange
        var activeItem = CreateCatalogItem(brand: "Essilor", name: "Active Lens");
        var inactiveItem = CreateCatalogItem(brand: "Hoya", name: "Inactive Lens");
        inactiveItem.Deactivate();

        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem> { activeItem });

        var query = new GetLensCatalogQuery(IncludeInactive: false);

        // Act
        var result = await GetLensCatalogHandler.Handle(query, repository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Brand.Should().Be("Essilor");
        result[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetLensCatalog_IncludeInactive_ReturnsAllItems()
    {
        // Arrange
        var activeItem = CreateCatalogItem(brand: "Essilor");
        var inactiveItem = CreateCatalogItem(brand: "Hoya");
        inactiveItem.Deactivate();

        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetAllAsync(true, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem> { activeItem, inactiveItem });

        var query = new GetLensCatalogQuery(IncludeInactive: true);

        // Act
        var result = await GetLensCatalogHandler.Handle(query, repository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLensCatalog_MapsStockEntries()
    {
        // Arrange
        var item = CreateCatalogItem();
        item.AddStockEntry(sph: -2.00m, cyl: 0m, add: null, quantity: 5);
        item.AddStockEntry(sph: -1.50m, cyl: -0.75m, add: null, quantity: 3);

        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem> { item });

        var query = new GetLensCatalogQuery(IncludeInactive: false);

        // Act
        var result = await GetLensCatalogHandler.Handle(query, repository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].StockEntries.Should().HaveCount(2);
        result[0].StockEntries[0].Sph.Should().Be(-2.00m);
        result[0].StockEntries[1].Sph.Should().Be(-1.50m);
    }

    [Fact]
    public async Task GetLensCatalog_EmptyCatalog_ReturnsEmptyList()
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem>());

        var query = new GetLensCatalogQuery();

        // Act
        var result = await GetLensCatalogHandler.Handle(query, repository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    // ─── AdjustLensStock Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task AdjustLensStock_ExistingEntry_IncreasesQuantity()
    {
        // Arrange
        var item = CreateCatalogItem();
        item.AddStockEntry(sph: -2.00m, cyl: 0m, add: null, quantity: 5);
        var existingEntry = item.StockEntries[0];

        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        repository.GetStockEntryAsync(item.Id, -2.00m, 0m, null, Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var unitOfWork = Substitute.For<IUnitOfWork>();

        var command = new AdjustLensStockCommand(
            LensCatalogItemId: item.Id,
            Sph: -2.00m,
            Cyl: 0m,
            Add: null,
            QuantityChange: 10,
            Reason: "Restock");

        // Act
        var result = await AdjustLensStockHandler.Handle(
            command, repository, unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Quantity.Should().Be(15);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustLensStock_NoExistingEntry_PositiveChange_CreatesNewEntry()
    {
        // Arrange
        var item = CreateCatalogItem();
        // No stock entries yet

        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        repository.GetStockEntryAsync(item.Id, -3.00m, -0.75m, null, Arg.Any<CancellationToken>())
            .Returns((LensStockEntry?)null);

        var unitOfWork = Substitute.For<IUnitOfWork>();

        var command = new AdjustLensStockCommand(
            LensCatalogItemId: item.Id,
            Sph: -3.00m,
            Cyl: -0.75m,
            Add: null,
            QuantityChange: 8,
            Reason: "Initial stock");

        // Act
        var result = await AdjustLensStockHandler.Handle(
            command, repository, unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Quantity.Should().Be(8);
        result.Value.Sph.Should().Be(-3.00m);
        result.Value.Cyl.Should().Be(-0.75m);
        item.StockEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task AdjustLensStock_NoExistingEntry_NegativeChange_ReturnsError()
    {
        // Arrange
        var item = CreateCatalogItem();

        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        repository.GetStockEntryAsync(item.Id, -2.00m, 0m, null, Arg.Any<CancellationToken>())
            .Returns((LensStockEntry?)null);

        var unitOfWork = Substitute.For<IUnitOfWork>();

        var command = new AdjustLensStockCommand(
            LensCatalogItemId: item.Id,
            Sph: -2.00m,
            Cyl: 0m,
            Add: null,
            QuantityChange: -5,
            Reason: "Deduct from non-existent stock");

        // Act
        var result = await AdjustLensStockHandler.Handle(
            command, repository, unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustLensStock_ExistingEntry_WouldGoNegative_ReturnsError()
    {
        // Arrange
        var item = CreateCatalogItem();
        item.AddStockEntry(sph: -2.00m, cyl: 0m, add: null, quantity: 3);
        var existingEntry = item.StockEntries[0];

        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        repository.GetStockEntryAsync(item.Id, -2.00m, 0m, null, Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var unitOfWork = Substitute.For<IUnitOfWork>();

        var command = new AdjustLensStockCommand(
            LensCatalogItemId: item.Id,
            Sph: -2.00m,
            Cyl: 0m,
            Add: null,
            QuantityChange: -10,  // would result in -7
            Reason: "Overdeduct");

        // Act
        var result = await AdjustLensStockHandler.Handle(
            command, repository, unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustLensStock_CatalogItemNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((LensCatalogItem?)null);

        var unitOfWork = Substitute.For<IUnitOfWork>();

        var command = new AdjustLensStockCommand(
            LensCatalogItemId: Guid.NewGuid(),
            Sph: -2.00m,
            Cyl: 0m,
            Add: null,
            QuantityChange: 5,
            Reason: "Restock");

        // Act
        var result = await AdjustLensStockHandler.Handle(
            command, repository, unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task AdjustLensStock_WithAddPower_CreatesEntryWithAddPower()
    {
        // Arrange
        var item = CreateCatalogItem(lensType: "progressive");

        var repository = Substitute.For<ILensCatalogRepository>();
        repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        repository.GetStockEntryAsync(item.Id, -1.00m, 0m, 2.00m, Arg.Any<CancellationToken>())
            .Returns((LensStockEntry?)null);

        var unitOfWork = Substitute.For<IUnitOfWork>();

        var command = new AdjustLensStockCommand(
            LensCatalogItemId: item.Id,
            Sph: -1.00m,
            Cyl: 0m,
            Add: 2.00m,
            QuantityChange: 4,
            Reason: "Stock progressive lenses");

        // Act
        var result = await AdjustLensStockHandler.Handle(
            command, repository, unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Add.Should().Be(2.00m);
        result.Value.Quantity.Should().Be(4);
    }
}
