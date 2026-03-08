using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Optical.Application.Features.Combos;
using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// Unit tests for Combo package handlers: CreateComboPackage, UpdateComboPackage, GetComboPackages.
/// </summary>
public class ComboHandlerTests
{
    private static readonly BranchId TestBranchId = new(Guid.NewGuid());
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestBranchGuid = TestBranchId.Value;

    private static ICurrentUser CreateCurrentUser()
    {
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(TestUserId);
        user.BranchId.Returns(TestBranchGuid);
        return user;
    }

    private static IComboPackageRepository CreateRepository()
        => Substitute.For<IComboPackageRepository>();

    private static IFrameRepository CreateFrameRepository()
        => Substitute.For<IFrameRepository>();

    private static ILensCatalogRepository CreateLensCatalogRepository()
        => Substitute.For<ILensCatalogRepository>();

    private static IUnitOfWork CreateUnitOfWork()
        => Substitute.For<IUnitOfWork>();

    private static ComboPackage CreateComboPackage(
        string name = "Test Combo",
        decimal comboPrice = 5_000_000m)
        => ComboPackage.Create(name, null, null, null, comboPrice, null, TestBranchId);

    // ========================
    // CreateComboPackage Tests
    // ========================

    [Fact]
    public async Task CreateComboPackage_ValidCommand_ReturnsSuccessWithId()
    {
        // Arrange
        var repository = CreateRepository();
        var unitOfWork = CreateUnitOfWork();
        var currentUser = CreateCurrentUser();
        var validator = new CreateComboPackageCommandValidator();
        var command = new CreateComboPackageCommand(
            Name: "Ray-Ban + Essilor SV",
            Description: "Classic combo for single vision",
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: 8_000_000m,
            OriginalTotalPrice: 10_000_000m);

        // Act
        var result = await CreateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        repository.Received(1).Add(Arg.Any<ComboPackage>());
        await unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateComboPackage_EmptyName_ReturnsValidationFailure()
    {
        // Arrange
        var repository = CreateRepository();
        var unitOfWork = CreateUnitOfWork();
        var currentUser = CreateCurrentUser();
        var validator = new CreateComboPackageCommandValidator();
        var command = new CreateComboPackageCommand(
            Name: "",
            Description: null,
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: 5_000_000m,
            OriginalTotalPrice: null);

        // Act
        var result = await CreateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        repository.DidNotReceive().Add(Arg.Any<ComboPackage>());
    }

    [Fact]
    public async Task CreateComboPackage_NegativeComboPrice_ReturnsValidationFailure()
    {
        // Arrange
        var repository = CreateRepository();
        var unitOfWork = CreateUnitOfWork();
        var currentUser = CreateCurrentUser();
        var validator = new CreateComboPackageCommandValidator();
        var command = new CreateComboPackageCommand(
            Name: "Test Combo",
            Description: null,
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: -1000m,
            OriginalTotalPrice: null);

        // Act
        var result = await CreateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task CreateComboPackage_ZeroComboPrice_ReturnsValidationFailure()
    {
        // Arrange
        var repository = CreateRepository();
        var unitOfWork = CreateUnitOfWork();
        var currentUser = CreateCurrentUser();
        var validator = new CreateComboPackageCommandValidator();
        var command = new CreateComboPackageCommand(
            Name: "Test Combo",
            Description: null,
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: 0m,
            OriginalTotalPrice: null);

        // Act
        var result = await CreateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task CreateComboPackage_NameTooLong_ReturnsValidationFailure()
    {
        // Arrange
        var repository = CreateRepository();
        var unitOfWork = CreateUnitOfWork();
        var currentUser = CreateCurrentUser();
        var validator = new CreateComboPackageCommandValidator();
        var command = new CreateComboPackageCommand(
            Name: new string('A', 201),
            Description: null,
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: 5_000_000m,
            OriginalTotalPrice: null);

        // Act
        var result = await CreateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // ========================
    // UpdateComboPackage Tests
    // ========================

    [Fact]
    public async Task UpdateComboPackage_ExistingPackage_ReturnsSuccess()
    {
        // Arrange
        var existingCombo = CreateComboPackage();
        var repository = CreateRepository();
        repository.GetByIdAsync(existingCombo.Id, CancellationToken.None)
            .Returns(existingCombo);
        var unitOfWork = CreateUnitOfWork();
        var validator = new UpdateComboPackageCommandValidator();
        var command = new UpdateComboPackageCommand(
            Id: existingCombo.Id,
            Name: "Updated Combo",
            Description: "Updated description",
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: 9_000_000m,
            OriginalTotalPrice: 12_000_000m,
            IsActive: true);

        // Act
        var result = await UpdateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingCombo.Name.Should().Be("Updated Combo");
        existingCombo.ComboPrice.Should().Be(9_000_000m);
        await unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateComboPackage_NotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var repository = CreateRepository();
        repository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((ComboPackage?)null);
        var unitOfWork = CreateUnitOfWork();
        var validator = new UpdateComboPackageCommandValidator();
        var command = new UpdateComboPackageCommand(
            Id: Guid.NewGuid(),
            Name: "Test Combo",
            Description: null,
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: 5_000_000m,
            OriginalTotalPrice: null,
            IsActive: true);

        // Act
        var result = await UpdateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task UpdateComboPackage_EmptyName_ReturnsValidationFailure()
    {
        // Arrange
        var repository = CreateRepository();
        var unitOfWork = CreateUnitOfWork();
        var validator = new UpdateComboPackageCommandValidator();
        var command = new UpdateComboPackageCommand(
            Id: Guid.NewGuid(),
            Name: "",
            Description: null,
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: 5_000_000m,
            OriginalTotalPrice: null,
            IsActive: true);

        // Act
        var result = await UpdateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None);
    }

    [Fact]
    public async Task UpdateComboPackage_Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var existingCombo = CreateComboPackage();
        var repository = CreateRepository();
        repository.GetByIdAsync(existingCombo.Id, CancellationToken.None)
            .Returns(existingCombo);
        var unitOfWork = CreateUnitOfWork();
        var validator = new UpdateComboPackageCommandValidator();
        var command = new UpdateComboPackageCommand(
            Id: existingCombo.Id,
            Name: "Test Combo",
            Description: null,
            FrameId: null,
            LensCatalogItemId: null,
            ComboPrice: 5_000_000m,
            OriginalTotalPrice: null,
            IsActive: false);

        // Act
        var result = await UpdateComboPackageHandler.Handle(
            command, repository, unitOfWork, validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingCombo.IsActive.Should().BeFalse();
    }

    // ========================
    // GetComboPackages Tests
    // ========================

    [Fact]
    public async Task GetComboPackages_ActiveOnly_ReturnsActivePackages()
    {
        // Arrange
        var activeCombo = CreateComboPackage("Active Combo", 5_000_000m);
        var repository = CreateRepository();
        var frameRepository = CreateFrameRepository();
        var lensRepository = CreateLensCatalogRepository();
        repository.GetAllAsync(false, CancellationToken.None)
            .Returns([activeCombo]);
        var query = new GetComboPackagesQuery(IncludeInactive: false);

        // Act
        var result = await GetComboPackagesHandler.Handle(
            query, repository, frameRepository, lensRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Combo");
        result[0].ComboPrice.Should().Be(5_000_000m);
    }

    [Fact]
    public async Task GetComboPackages_IncludeInactive_PassesFlagToRepository()
    {
        // Arrange
        var repository = CreateRepository();
        var frameRepository = CreateFrameRepository();
        var lensRepository = CreateLensCatalogRepository();
        repository.GetAllAsync(true, CancellationToken.None).Returns([]);
        var query = new GetComboPackagesQuery(IncludeInactive: true);

        // Act
        var result = await GetComboPackagesHandler.Handle(
            query, repository, frameRepository, lensRepository, CancellationToken.None);

        // Assert
        await repository.Received(1).GetAllAsync(true, CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetComboPackages_WithFrameId_LoadsFrameName()
    {
        // Arrange
        var frameId = Guid.NewGuid();
        var frame = Frame.Create(
            "Ray-Ban", "RB3025", "Black", 52, 18, 140,
            FrameMaterial.Metal, FrameType.FullRim, FrameGender.Unisex,
            2_500_000m, 1_200_000m, null, TestBranchId);

        var combo = ComboPackage.Create("Combo with Frame", null, frameId, null, 5_000_000m, null, TestBranchId);
        var repository = CreateRepository();
        var frameRepository = CreateFrameRepository();
        var lensRepository = CreateLensCatalogRepository();
        repository.GetAllAsync(false, CancellationToken.None).Returns([combo]);
        frameRepository.GetByIdAsync(frameId, CancellationToken.None).Returns(frame);
        var query = new GetComboPackagesQuery(IncludeInactive: false);

        // Act
        var result = await GetComboPackagesHandler.Handle(
            query, repository, frameRepository, lensRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].FrameId.Should().Be(frameId);
        result[0].FrameName.Should().Be("Ray-Ban RB3025");
    }

    [Fact]
    public async Task GetComboPackages_WithLensId_LoadsLensName()
    {
        // Arrange
        var lensId = Guid.NewGuid();
        var lens = LensCatalogItem.Create(
            "Essilor", "Crizal Single Vision", "single_vision",
            LensMaterial.HiIndex, LensCoating.AntiReflective,
            3_000_000m, 1_500_000m, null, TestBranchId);

        var combo = ComboPackage.Create("Combo with Lens", null, null, lensId, 5_000_000m, null, TestBranchId);
        var repository = CreateRepository();
        var frameRepository = CreateFrameRepository();
        var lensRepository = CreateLensCatalogRepository();
        repository.GetAllAsync(false, CancellationToken.None).Returns([combo]);
        lensRepository.GetByIdAsync(lensId, CancellationToken.None).Returns(lens);
        var query = new GetComboPackagesQuery(IncludeInactive: false);

        // Act
        var result = await GetComboPackagesHandler.Handle(
            query, repository, frameRepository, lensRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].LensCatalogItemId.Should().Be(lensId);
        result[0].LensName.Should().Be("Essilor Crizal Single Vision");
    }

    [Fact]
    public async Task GetComboPackages_MapsToCorrectDto()
    {
        // Arrange
        var combo = ComboPackage.Create(
            "Full Combo",
            "A complete glasses combo",
            null,
            null,
            8_000_000m,
            10_000_000m,
            TestBranchId);
        var repository = CreateRepository();
        var frameRepository = CreateFrameRepository();
        var lensRepository = CreateLensCatalogRepository();
        repository.GetAllAsync(false, CancellationToken.None).Returns([combo]);
        var query = new GetComboPackagesQuery(IncludeInactive: false);

        // Act
        var result = await GetComboPackagesHandler.Handle(
            query, repository, frameRepository, lensRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be(combo.Id);
        dto.Name.Should().Be("Full Combo");
        dto.Description.Should().Be("A complete glasses combo");
        dto.ComboPrice.Should().Be(8_000_000m);
        dto.OriginalTotalPrice.Should().Be(10_000_000m);
        dto.Savings.Should().Be(2_000_000m);
        dto.IsActive.Should().BeTrue();
    }
}
