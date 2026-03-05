using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Pharmacy.Application.Features;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

public class DrugCatalogCrudHandlerTests
{
    private readonly IDrugCatalogItemRepository _repository = Substitute.For<IDrugCatalogItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateDrugCatalogItemCommand> _createValidator = Substitute.For<IValidator<CreateDrugCatalogItemCommand>>();
    private readonly IValidator<UpdateDrugCatalogItemCommand> _updateValidator = Substitute.For<IValidator<UpdateDrugCatalogItemCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public DrugCatalogCrudHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    private void SetupValidCreateValidator()
    {
        _createValidator.ValidateAsync(Arg.Any<CreateDrugCatalogItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidUpdateValidator()
    {
        _updateValidator.ValidateAsync(Arg.Any<UpdateDrugCatalogItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static CreateDrugCatalogItemCommand CreateValidCreateCommand() =>
        new(
            Name: "Tobramycin 0.3%",
            NameVi: "Tobramycin 0,3%",
            GenericName: "Tobramycin",
            Form: (int)DrugForm.EyeDrops,
            Strength: "0.3%",
            Route: (int)DrugRoute.Topical,
            Unit: "Chai",
            DefaultDosageTemplate: "1-2 drops x 4 times/day");

    private static UpdateDrugCatalogItemCommand CreateValidUpdateCommand(Guid? id = null) =>
        new(
            Id: id ?? Guid.NewGuid(),
            Name: "Tobramycin 0.3% Updated",
            NameVi: "Tobramycin 0,3% (C\u1eadp nh\u1eadt)",
            GenericName: "Tobramycin",
            Form: (int)DrugForm.EyeDrops,
            Strength: "0.3%",
            Route: (int)DrugRoute.Topical,
            Unit: "Chai",
            DefaultDosageTemplate: "1-2 drops x 3 times/day");

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsSuccessWithId()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = CreateValidCreateCommand();

        // Act
        var result = await CreateDrugCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repository.Received(1).Add(Arg.Any<DrugCatalogItem>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WithMissingName_ReturnsValidationError()
    {
        // Arrange
        var command = CreateValidCreateCommand() with { Name = "" };
        _createValidator.ValidateAsync(Arg.Any<CreateDrugCatalogItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Drug name is required.")
            }));

        // Act
        var result = await CreateDrugCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Create_WithMissingNameVi_ReturnsValidationError()
    {
        // Arrange
        var command = CreateValidCreateCommand() with { NameVi = "" };
        _createValidator.ValidateAsync(Arg.Any<CreateDrugCatalogItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("NameVi", "Vietnamese drug name is required.")
            }));

        // Act
        var result = await CreateDrugCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Create_WithMissingUnit_ReturnsValidationError()
    {
        // Arrange
        var command = CreateValidCreateCommand() with { Unit = "" };
        _createValidator.ValidateAsync(Arg.Any<CreateDrugCatalogItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Unit", "Unit is required.")
            }));

        // Act
        var result = await CreateDrugCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidData_UpdatesEntity()
    {
        // Arrange
        SetupValidUpdateValidator();
        var itemId = Guid.NewGuid();
        var existingItem = DrugCatalogItem.Create(
            "Tobramycin 0.3%", "Tobramycin 0,3%", "Tobramycin",
            DrugForm.EyeDrops, "0.3%", DrugRoute.Topical, "Chai",
            "1-2 drops x 4 times/day", new BranchId(DefaultBranchId));

        _repository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(existingItem);

        var command = CreateValidUpdateCommand(itemId);

        // Act
        var result = await UpdateDrugCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        SetupValidUpdateValidator();
        var nonExistentId = Guid.NewGuid();
        _repository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((DrugCatalogItem?)null);

        var command = CreateValidUpdateCommand(nonExistentId);

        // Act
        var result = await UpdateDrugCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Update_WithInvalidData_ReturnsValidationError()
    {
        // Arrange
        var command = CreateValidUpdateCommand() with { Name = "" };
        _updateValidator.ValidateAsync(Arg.Any<UpdateDrugCatalogItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Drug name is required.")
            }));

        // Act
        var result = await UpdateDrugCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    #endregion
}
