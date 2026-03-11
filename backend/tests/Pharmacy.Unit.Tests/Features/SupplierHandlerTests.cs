using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Pharmacy.Application.Features.Suppliers;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for Supplier CRUD handlers (CreateSupplier, UpdateSupplier, GetSuppliers).
/// Follows the Wolverine static handler pattern established in DrugCatalogCrudHandlerTests.
/// </summary>
public class SupplierHandlerTests
{
    private readonly ISupplierRepository _repository = Substitute.For<ISupplierRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateSupplierCommand> _createValidator = Substitute.For<IValidator<CreateSupplierCommand>>();
    private readonly IValidator<UpdateSupplierCommand> _updateValidator = Substitute.For<IValidator<UpdateSupplierCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public SupplierHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    private void SetupValidCreateValidator()
    {
        _createValidator.ValidateAsync(Arg.Any<CreateSupplierCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidUpdateValidator()
    {
        _updateValidator.ValidateAsync(Arg.Any<UpdateSupplierCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static CreateSupplierCommand CreateValidCreateCommand() =>
        new(
            Name: "Công ty Dược Phẩm ABC",
            ContactInfo: "123 Nguyễn Văn Linh, TP.HCM",
            Phone: "0901234567",
            Email: "contact@abc-pharma.vn");

    private static UpdateSupplierCommand CreateValidUpdateCommand(Guid? id = null) =>
        new(
            Id: id ?? Guid.NewGuid(),
            Name: "Công ty Dược Phẩm ABC (Cập nhật)",
            ContactInfo: "456 Lê Lợi, TP.HCM",
            Phone: "0909876543",
            Email: "updated@abc-pharma.vn");

    #region CreateSupplier Tests

    [Fact]
    public async Task CreateSupplier_ValidInput_ReturnsSuccess()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = CreateValidCreateCommand();

        // Act
        var result = await CreateSupplierHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repository.Received(1).Add(Arg.Any<Supplier>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSupplier_EmptyName_ReturnsValidationError()
    {
        // Arrange
        var command = CreateValidCreateCommand() with { Name = "" };
        _createValidator.ValidateAsync(Arg.Any<CreateSupplierCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Supplier name is required.")
            }));

        // Act
        var result = await CreateSupplierHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        _repository.DidNotReceive().Add(Arg.Any<Supplier>());
    }

    #endregion

    #region UpdateSupplier Tests

    [Fact]
    public async Task UpdateSupplier_Existing_UpdatesFields()
    {
        // Arrange
        SetupValidUpdateValidator();
        var supplierId = Guid.NewGuid();
        var existingSupplier = Supplier.Create(
            "Công ty Dược Phẩm ABC",
            "123 Nguyễn Văn Linh",
            "0901234567",
            "contact@abc-pharma.vn",
            new BranchId(DefaultBranchId));

        _repository.GetByIdAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(existingSupplier);

        var command = CreateValidUpdateCommand(supplierId);

        // Act
        var result = await UpdateSupplierHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        existingSupplier.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task UpdateSupplier_NotFound_ReturnsError()
    {
        // Arrange
        SetupValidUpdateValidator();
        var nonExistentId = Guid.NewGuid();
        _repository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Supplier?)null);

        var command = CreateValidUpdateCommand(nonExistentId);

        // Act
        var result = await UpdateSupplierHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region ToggleSupplierActive Tests

    [Fact]
    public async Task ToggleSupplierActive_ActiveSupplier_Deactivates()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var supplier = Supplier.Create(
            "Active Supplier",
            "Address",
            "0901234567",
            "test@test.vn",
            new BranchId(DefaultBranchId));

        supplier.IsActive.Should().BeTrue(); // precondition

        _repository.GetByIdAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(supplier);

        var command = new ToggleSupplierActiveCommand(supplierId);

        // Act
        var result = await ToggleSupplierActiveHandler.Handle(
            command, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        supplier.IsActive.Should().BeFalse();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ToggleSupplierActive_InactiveSupplier_Activates()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var supplier = Supplier.Create(
            "Inactive Supplier",
            "Address",
            "0901234567",
            "test@test.vn",
            new BranchId(DefaultBranchId));
        supplier.Deactivate(); // make inactive first
        supplier.IsActive.Should().BeFalse(); // precondition

        _repository.GetByIdAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(supplier);

        var command = new ToggleSupplierActiveCommand(supplierId);

        // Act
        var result = await ToggleSupplierActiveHandler.Handle(
            command, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        supplier.IsActive.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ToggleSupplierActive_NotFound_ReturnsError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Supplier?)null);

        var command = new ToggleSupplierActiveCommand(nonExistentId);

        // Act
        var result = await ToggleSupplierActiveHandler.Handle(
            command, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetSuppliers Tests

    [Fact]
    public async Task GetSuppliers_ReturnsActiveSuppliers()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            Supplier.Create("Supplier A", "Address A", "0901111111", "a@test.vn", new BranchId(DefaultBranchId)),
            Supplier.Create("Supplier B", "Address B", "0902222222", "b@test.vn", new BranchId(DefaultBranchId))
        };

        _repository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(suppliers);

        var query = new GetSuppliersQuery();

        // Act
        var result = await GetSuppliersHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.IsActive.Should().BeTrue());
        result[0].Name.Should().Be("Supplier A");
        result[1].Name.Should().Be("Supplier B");
    }

    #endregion
}
