using Billing.Application.Features.ServiceCatalog;
using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Application;
using Shared.Domain;

namespace Billing.Unit.Tests.Features;

public class ServiceCatalogHandlerTests
{
    private readonly IServiceCatalogRepository _repository = Substitute.For<IServiceCatalogRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private readonly IValidator<CreateServiceCatalogItemCommand> _createValidator =
        Substitute.For<IValidator<CreateServiceCatalogItemCommand>>();

    private readonly IValidator<UpdateServiceCatalogItemCommand> _updateValidator =
        Substitute.For<IValidator<UpdateServiceCatalogItemCommand>>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public ServiceCatalogHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
        _currentUser.UserId.Returns(DefaultUserId);
    }

    private void SetupValidCreateValidator()
    {
        _createValidator
            .ValidateAsync(Arg.Any<CreateServiceCatalogItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidUpdateValidator()
    {
        _updateValidator
            .ValidateAsync(Arg.Any<UpdateServiceCatalogItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    #region CreateServiceCatalogItem Tests

    [Fact]
    public async Task CreateServiceCatalogItem_ValidInput_CreatesItemWithCorrectFields()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = new CreateServiceCatalogItemCommand(
            "CONSULTATION", "Consultation", "Kham benh", 150000, null);

        _repository.GetActiveByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ServiceCatalogItem?)null);

        // Act
        var result = await CreateServiceCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Code.Should().Be("CONSULTATION");
        result.Value.Name.Should().Be("Consultation");
        result.Value.NameVi.Should().Be("Kham benh");
        result.Value.Price.Should().Be(150000);
        result.Value.IsActive.Should().BeTrue();
        _repository.Received(1).Add(Arg.Any<ServiceCatalogItem>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateServiceCatalogItem_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = new CreateServiceCatalogItemCommand(
            "CONSULTATION", "Consultation", "Kham benh", 150000, null);

        var existing = ServiceCatalogItem.Create(
            "CONSULTATION", "Existing", "Existing", 100000, new BranchId(DefaultBranchId));
        _repository.GetActiveByCodeAsync("CONSULTATION", Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await CreateServiceCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Conflict");
    }

    #endregion

    #region UpdateServiceCatalogItem Tests

    [Fact]
    public async Task UpdateServiceCatalogItem_ValidInput_UpdatesNamePriceFields()
    {
        // Arrange
        SetupValidUpdateValidator();
        var itemId = Guid.NewGuid();
        var existing = ServiceCatalogItem.Create(
            "CONSULTATION", "Old Name", "Ten cu", 100000, new BranchId(DefaultBranchId));

        // Use reflection to set Id for testing
        typeof(Entity).GetProperty("Id")!.SetValue(existing, itemId);

        _repository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new UpdateServiceCatalogItemCommand(
            itemId, "New Name", "Ten moi", 200000, true, "Updated desc");

        // Act
        var result = await UpdateServiceCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.NameVi.Should().Be("Ten moi");
        result.Value.Price.Should().Be(200000);
        result.Value.Description.Should().Be("Updated desc");
        _repository.Received(1).Update(Arg.Any<ServiceCatalogItem>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateServiceCatalogItem_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidUpdateValidator();
        var itemId = Guid.NewGuid();
        _repository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns((ServiceCatalogItem?)null);

        var command = new UpdateServiceCatalogItemCommand(
            itemId, "Name", "Ten", 100000, true, null);

        // Act
        var result = await UpdateServiceCatalogItemHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    #endregion

    #region GetServiceCatalogItems Tests

    [Fact]
    public async Task GetServiceCatalogItems_Default_ReturnsOnlyActiveItems()
    {
        // Arrange
        var items = new List<ServiceCatalogItem>
        {
            ServiceCatalogItem.Create("CONSULTATION", "Consultation", "Kham benh", 150000, new BranchId(DefaultBranchId)),
            ServiceCatalogItem.Create("FOLLOWUP", "Follow-up", "Tai kham", 100000, new BranchId(DefaultBranchId))
        };

        _repository.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(items);

        var query = new GetServiceCatalogItemsQuery(false);

        // Act
        var result = await GetServiceCatalogItemsHandler.Handle(
            query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Code.Should().Be("CONSULTATION");
        result.Value[1].Code.Should().Be("FOLLOWUP");
    }

    [Fact]
    public async Task GetServiceCatalogItems_IncludeInactive_ReturnsAll()
    {
        // Arrange
        var active = ServiceCatalogItem.Create(
            "CONSULTATION", "Consultation", "Kham benh", 150000, new BranchId(DefaultBranchId));
        var inactive = ServiceCatalogItem.Create(
            "OLDSERVICE", "Old Service", "Dich vu cu", 50000, new BranchId(DefaultBranchId));
        inactive.Deactivate();

        _repository.GetAllAsync(true, Arg.Any<CancellationToken>())
            .Returns(new List<ServiceCatalogItem> { active, inactive });

        var query = new GetServiceCatalogItemsQuery(true);

        // Act
        var result = await GetServiceCatalogItemsHandler.Handle(
            query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    #endregion

    #region GetServiceCatalogItemByCode Tests

    [Fact]
    public async Task GetServiceCatalogItemByCode_Found_ReturnsItem()
    {
        // Arrange
        var item = ServiceCatalogItem.Create(
            "CONSULTATION", "Consultation", "Kham benh", 150000, new BranchId(DefaultBranchId));

        _repository.GetActiveByCodeAsync("CONSULTATION", Arg.Any<CancellationToken>())
            .Returns(item);

        var query = new GetServiceCatalogItemByCodeQuery("CONSULTATION");

        // Act
        var result = await GetServiceCatalogItemByCodeHandler.Handle(
            query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be("CONSULTATION");
        result.Value.Price.Should().Be(150000);
    }

    [Fact]
    public async Task GetServiceCatalogItemByCode_LowercaseInput_NormalizesToUppercase()
    {
        // Arrange
        var item = ServiceCatalogItem.Create(
            "CONSULTATION", "Consultation", "Kham benh", 150000, new BranchId(DefaultBranchId));

        _repository.GetActiveByCodeAsync("CONSULTATION", Arg.Any<CancellationToken>())
            .Returns(item);

        var query = new GetServiceCatalogItemByCodeQuery("consultation"); // lowercase input

        // Act
        var result = await GetServiceCatalogItemByCodeHandler.Handle(
            query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be("CONSULTATION");
        // Verify the repository was called with uppercase
        await _repository.Received(1).GetActiveByCodeAsync("CONSULTATION", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetServiceCatalogItemByCode_NotFound_ReturnsNull()
    {
        // Arrange
        _repository.GetActiveByCodeAsync("NONEXISTENT", Arg.Any<CancellationToken>())
            .Returns((ServiceCatalogItem?)null);

        var query = new GetServiceCatalogItemByCodeQuery("NONEXISTENT");

        // Act
        var result = await GetServiceCatalogItemByCodeHandler.Handle(
            query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    #endregion
}
