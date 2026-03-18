using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Optical.Application.Features.Frames;
using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// Handler tests for Frame CRUD operations: CreateFrame, UpdateFrame, GetFrames, SearchFrames, GenerateBarcode.
/// </summary>
public class FrameHandlerTests
{
    private readonly IFrameRepository _repository = Substitute.For<IFrameRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateFrameCommand> _createValidator = Substitute.For<IValidator<CreateFrameCommand>>();
    private readonly IValidator<UpdateFrameCommand> _updateValidator = Substitute.For<IValidator<UpdateFrameCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public FrameHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    // ─── Helper builders ──────────────────────────────────────────────────────

    private void SetupValidCreateValidator()
    {
        _createValidator.ValidateAsync(Arg.Any<CreateFrameCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidUpdateValidator()
    {
        _updateValidator.ValidateAsync(Arg.Any<UpdateFrameCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static CreateFrameCommand ValidCreateCommand(string? barcode = null) =>
        new(
            Brand: "Ray-Ban",
            Model: "RB3025",
            Color: "Matte Black",
            LensWidth: 52,
            BridgeWidth: 18,
            TempleLength: 140,
            Material: (int)FrameMaterial.Metal,
            FrameType: (int)FrameType.FullRim,
            Gender: (int)FrameGender.Unisex,
            SellingPrice: 2_500_000m,
            CostPrice: 1_200_000m,
            Barcode: barcode,
            StockQuantity: 5,
            MinStockLevel: 2);

    private static UpdateFrameCommand ValidUpdateCommand(Guid? id = null, string? barcode = null) =>
        new(
            Id: id ?? Guid.NewGuid(),
            Brand: "Oakley",
            Model: "OX8156",
            Color: "Satin Black",
            LensWidth: 55,
            BridgeWidth: 17,
            TempleLength: 145,
            Material: (int)FrameMaterial.Titanium,
            FrameType: (int)FrameType.SemiRimless,
            Gender: (int)FrameGender.Male,
            SellingPrice: 3_000_000m,
            CostPrice: 1_500_000m,
            Barcode: barcode,
            StockQuantity: null,
            IsActive: true);

    private static Frame MakeFrame() =>
        Frame.Create(
            "Ray-Ban", "RB3025", "Matte Black",
            52, 18, 140,
            FrameMaterial.Metal, FrameType.FullRim, FrameGender.Unisex,
            2_500_000m, 1_200_000m, null,
            new BranchId(DefaultBranchId));

    // ─── CreateFrame Tests ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFrame_WithValidData_ReturnsSuccessWithGuid()
    {
        // Arrange
        SetupValidCreateValidator();
        _repository.IsBarcodeUniqueAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = ValidCreateCommand();

        // Act
        var result = await CreateFrameHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repository.Received(1).Add(Arg.Any<Frame>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFrame_WithValidBarcode_ChecksUniquenessAndSucceeds()
    {
        // Arrange
        SetupValidCreateValidator();
        var barcode = "5901234123457";
        _repository.IsBarcodeUniqueAsync(barcode, null, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = ValidCreateCommand(barcode);

        // Act
        var result = await CreateFrameHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).IsBarcodeUniqueAsync(barcode, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFrame_WithDuplicateBarcode_ReturnsConflictError()
    {
        // Arrange
        SetupValidCreateValidator();
        var barcode = "5901234123457";
        _repository.IsBarcodeUniqueAsync(barcode, null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = ValidCreateCommand(barcode);

        // Act
        var result = await CreateFrameHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
        _repository.DidNotReceive().Add(Arg.Any<Frame>());
    }

    [Fact]
    public async Task CreateFrame_WithValidationError_ReturnsValidationFailure()
    {
        // Arrange
        var command = ValidCreateCommand() with { Brand = "" };
        _createValidator.ValidateAsync(Arg.Any<CreateFrameCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Brand", "Brand is required.")
            }));

        // Act
        var result = await CreateFrameHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task CreateFrame_WithoutBarcode_DoesNotCheckUniqueness()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = ValidCreateCommand(barcode: null);

        // Act
        var result = await CreateFrameHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.DidNotReceive().IsBarcodeUniqueAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    // ─── UpdateFrame Tests ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateFrame_WithValidData_UpdatesAndReturnsSuccess()
    {
        // Arrange
        SetupValidUpdateValidator();
        var frameId = Guid.NewGuid();
        var existingFrame = MakeFrame();
        _repository.GetByIdAsync(frameId, Arg.Any<CancellationToken>())
            .Returns(existingFrame);

        var command = ValidUpdateCommand(frameId);

        // Act
        var result = await UpdateFrameHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateFrame_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        SetupValidUpdateValidator();
        var nonExistentId = Guid.NewGuid();
        _repository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Frame?)null);

        var command = ValidUpdateCommand(nonExistentId);

        // Act
        var result = await UpdateFrameHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task UpdateFrame_WithValidationError_ReturnsValidationFailure()
    {
        // Arrange
        var command = ValidUpdateCommand() with { Brand = "" };
        _updateValidator.ValidateAsync(Arg.Any<UpdateFrameCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Brand", "Brand is required.")
            }));

        // Act
        var result = await UpdateFrameHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task UpdateFrame_WithDuplicateBarcode_ReturnsConflictError()
    {
        // Arrange
        SetupValidUpdateValidator();
        var frameId = Guid.NewGuid();
        var existingFrame = MakeFrame();
        var barcode = "5901234123457";

        _repository.GetByIdAsync(frameId, Arg.Any<CancellationToken>())
            .Returns(existingFrame);
        _repository.IsBarcodeUniqueAsync(barcode, existingFrame.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = ValidUpdateCommand(frameId, barcode);

        // Act
        var result = await UpdateFrameHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }

    [Fact]
    public async Task UpdateFrame_WithSameBarcode_ExcludesSelfInUniquenessCheck()
    {
        // Arrange
        SetupValidUpdateValidator();
        var frameId = Guid.NewGuid();
        var barcode = "5901234123457";
        var existingFrame = MakeFrame();

        _repository.GetByIdAsync(frameId, Arg.Any<CancellationToken>())
            .Returns(existingFrame);
        _repository.IsBarcodeUniqueAsync(barcode, existingFrame.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = ValidUpdateCommand(frameId, barcode);

        // Act
        var result = await UpdateFrameHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).IsBarcodeUniqueAsync(barcode, existingFrame.Id, Arg.Any<CancellationToken>());
    }

    // ─── GetFrames Tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetFrames_ReturnsPaginatedFrameList()
    {
        // Arrange
        var frames = new List<Frame> { MakeFrame(), MakeFrame() };
        _repository.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(frames);

        var query = new GetFramesQuery(IncludeInactive: false, Page: 1, PageSize: 20);

        // Act
        var result = await GetFramesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetFrames_WithIncludeInactive_PassesFlagToRepository()
    {
        // Arrange
        _repository.GetAllAsync(true, Arg.Any<CancellationToken>())
            .Returns(new List<Frame>());

        var query = new GetFramesQuery(IncludeInactive: true, Page: 1, PageSize: 20);

        // Act
        var result = await GetFramesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetAllAsync(true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFrames_MapsFramesToFrameSummaryDto()
    {
        // Arrange
        var frame = MakeFrame();
        _repository.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<Frame> { frame });

        var query = new GetFramesQuery(IncludeInactive: false, Page: 1, PageSize: 20);

        // Act
        var result = await GetFramesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value.Items.First();
        dto.Id.Should().Be(frame.Id);
        dto.Brand.Should().Be(frame.Brand);
        dto.Model.Should().Be(frame.Model);
        dto.Color.Should().Be(frame.Color);
        dto.SizeDisplay.Should().Be(frame.SizeDisplay);
        dto.SellingPrice.Should().Be(frame.SellingPrice);
        dto.IsActive.Should().Be(frame.IsActive);
    }

    // ─── SearchFrames Tests ───────────────────────────────────────────────────

    [Fact]
    public async Task SearchFrames_ReturnsFilteredPaginatedResults()
    {
        // Arrange
        var frames = new List<Frame> { MakeFrame() };
        _repository.SearchAsync("Ray-Ban", null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(frames);
        _repository.GetTotalCountAsync("Ray-Ban", null, null, null, Arg.Any<CancellationToken>())
            .Returns(1);

        var query = new SearchFramesQuery(SearchTerm: "Ray-Ban", Material: null, FrameType: null, Gender: null, Page: 1, PageSize: 20);

        // Act
        var result = await SearchFramesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchFrames_WithMaterialFilter_PassesFilterToRepository()
    {
        // Arrange
        _repository.SearchAsync(null, (int)FrameMaterial.Metal, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new List<Frame>());
        _repository.GetTotalCountAsync(null, (int)FrameMaterial.Metal, null, null, Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new SearchFramesQuery(SearchTerm: null, Material: (int)FrameMaterial.Metal, FrameType: null, Gender: null, Page: 1, PageSize: 20);

        // Act
        var result = await SearchFramesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).SearchAsync(null, (int)FrameMaterial.Metal, null, null, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchFrames_MapsFramesToFrameSummaryDto()
    {
        // Arrange
        var frame = MakeFrame();
        _repository.SearchAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Frame> { frame });
        _repository.GetTotalCountAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var query = new SearchFramesQuery(SearchTerm: "test", Material: null, FrameType: null, Gender: null, Page: 1, PageSize: 20);

        // Act
        var result = await SearchFramesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value.Items.First();
        dto.Id.Should().Be(frame.Id);
        dto.Brand.Should().Be(frame.Brand);
    }

    // ─── GenerateBarcode Tests ────────────────────────────────────────────────

    [Fact]
    public async Task GenerateBarcode_WithValidFrame_GeneratesAndAssignsBarcode()
    {
        // Arrange
        var frameId = Guid.NewGuid();
        var existingFrame = MakeFrame();
        _repository.GetByIdAsync(frameId, Arg.Any<CancellationToken>())
            .Returns(existingFrame);
        _repository.GetNextSequenceNumberAsync(Arg.Any<CancellationToken>())
            .Returns(42L);
        _repository.IsBarcodeUniqueAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new GenerateBarcodeCommand(frameId);

        // Act
        var result = await GenerateBarcodeHandler.Handle(
            command, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Should().HaveLength(13);
        result.Value.Should().MatchRegex(@"^\d{13}$");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateBarcode_WithNonExistentFrame_ReturnsNotFound()
    {
        // Arrange
        var frameId = Guid.NewGuid();
        _repository.GetByIdAsync(frameId, Arg.Any<CancellationToken>())
            .Returns((Frame?)null);

        var command = new GenerateBarcodeCommand(frameId);

        // Act
        var result = await GenerateBarcodeHandler.Handle(
            command, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GenerateBarcode_GeneratedBarcodeIsEan13Valid()
    {
        // Arrange
        var frameId = Guid.NewGuid();
        var existingFrame = MakeFrame();
        _repository.GetByIdAsync(frameId, Arg.Any<CancellationToken>())
            .Returns(existingFrame);
        _repository.GetNextSequenceNumberAsync(Arg.Any<CancellationToken>())
            .Returns(1L);
        _repository.IsBarcodeUniqueAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new GenerateBarcodeCommand(frameId);

        // Act
        var result = await GenerateBarcodeHandler.Handle(
            command, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Optical.Domain.Entities.Ean13Generator.IsValid(result.Value).Should().BeTrue();
    }
}
