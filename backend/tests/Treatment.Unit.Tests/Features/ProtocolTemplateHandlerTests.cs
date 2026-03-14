using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Features;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Entities;
using Treatment.Domain.Enums;

namespace Treatment.Unit.Tests.Features;

/// <summary>
/// Handler tests for Protocol Template CRUD operations:
/// CreateProtocolTemplate, UpdateProtocolTemplate, GetProtocolTemplates, GetProtocolTemplateById.
/// </summary>
public class ProtocolTemplateHandlerTests
{
    private readonly ITreatmentProtocolRepository _repository = Substitute.For<ITreatmentProtocolRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateProtocolTemplateCommand> _createValidator = Substitute.For<IValidator<CreateProtocolTemplateCommand>>();
    private readonly IValidator<UpdateProtocolTemplateCommand> _updateValidator = Substitute.For<IValidator<UpdateProtocolTemplateCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public ProtocolTemplateHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    // --- Helper builders ---

    private void SetupValidCreateValidator()
    {
        _createValidator.ValidateAsync(Arg.Any<CreateProtocolTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidUpdateValidator()
    {
        _updateValidator.ValidateAsync(Arg.Any<UpdateProtocolTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static CreateProtocolTemplateCommand ValidCreateCommand() =>
        new(
            Name: "Standard IPL 4-session",
            TreatmentType: (int)TreatmentType.IPL,
            DefaultSessionCount: 4,
            PricingMode: (int)PricingMode.PerPackage,
            DefaultPackagePrice: 5_000_000m,
            DefaultSessionPrice: 1_500_000m,
            MinIntervalDays: 14,
            MaxIntervalDays: 28,
            DefaultParametersJson: null,
            CancellationDeductionPercent: 15m,
            Description: "Standard IPL treatment protocol");

    private static UpdateProtocolTemplateCommand ValidUpdateCommand(Guid? id = null) =>
        new(
            Id: id ?? Guid.NewGuid(),
            Name: "Updated IPL 6-session",
            TreatmentType: (int)TreatmentType.IPL,
            DefaultSessionCount: 6,
            PricingMode: (int)PricingMode.PerSession,
            DefaultPackagePrice: 7_000_000m,
            DefaultSessionPrice: 1_200_000m,
            MinIntervalDays: 21,
            MaxIntervalDays: 35,
            DefaultParametersJson: "{\"pulse\":10}",
            CancellationDeductionPercent: 20m,
            Description: "Updated protocol");

    private static TreatmentProtocol MakeProtocol() =>
        TreatmentProtocol.Create(
            "Standard IPL 4-session",
            TreatmentType.IPL,
            4,
            PricingMode.PerPackage,
            5_000_000m,
            1_500_000m,
            14,
            28,
            null,
            15m,
            "Standard IPL treatment protocol",
            new BranchId(DefaultBranchId));

    // --- CreateProtocolTemplate Tests ---

    [Fact]
    public async Task CreateProtocolTemplate_WithValidData_ReturnsSuccessWithDto()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = ValidCreateCommand();

        // Act
        var result = await CreateProtocolTemplateHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Standard IPL 4-session");
        result.Value.DefaultSessionCount.Should().Be(4);
        _repository.Received(1).Add(Arg.Any<TreatmentProtocol>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProtocolTemplate_WithValidationError_ReturnsValidationFailure()
    {
        // Arrange
        var command = ValidCreateCommand() with { Name = "" };
        _createValidator.ValidateAsync(Arg.Any<CreateProtocolTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Name is required.")
            }));

        // Act
        var result = await CreateProtocolTemplateHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        _repository.DidNotReceive().Add(Arg.Any<TreatmentProtocol>());
    }

    [Fact]
    public async Task CreateProtocolTemplate_SetsBranchIdFromCurrentUser()
    {
        // Arrange
        SetupValidCreateValidator();
        TreatmentProtocol? capturedProtocol = null;
        _repository.When(r => r.Add(Arg.Any<TreatmentProtocol>()))
            .Do(ci => capturedProtocol = ci.Arg<TreatmentProtocol>());

        var command = ValidCreateCommand();

        // Act
        await CreateProtocolTemplateHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        capturedProtocol.Should().NotBeNull();
        capturedProtocol!.BranchId.Value.Should().Be(DefaultBranchId);
    }

    // --- CreateProtocolTemplate Validator Tests ---

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public async Task CreateProtocolTemplateValidator_InvalidSessionCount_Fails(int sessionCount)
    {
        // Arrange
        var validator = new CreateProtocolTemplateCommandValidator();
        var command = ValidCreateCommand() with { DefaultSessionCount = sessionCount };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DefaultSessionCount");
    }

    [Theory]
    [InlineData(9)]
    [InlineData(21)]
    public async Task CreateProtocolTemplateValidator_InvalidDeductionPercent_Fails(decimal percent)
    {
        // Arrange
        var validator = new CreateProtocolTemplateCommandValidator();
        var command = ValidCreateCommand() with { CancellationDeductionPercent = percent };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CancellationDeductionPercent");
    }

    [Fact]
    public async Task CreateProtocolTemplateValidator_EmptyName_Fails()
    {
        // Arrange
        var validator = new CreateProtocolTemplateCommandValidator();
        var command = ValidCreateCommand() with { Name = "" };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task CreateProtocolTemplateValidator_NegativePrice_Fails()
    {
        // Arrange
        var validator = new CreateProtocolTemplateCommandValidator();
        var command = ValidCreateCommand() with { DefaultPackagePrice = -1m };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DefaultPackagePrice");
    }

    [Fact]
    public async Task CreateProtocolTemplateValidator_ValidCommand_Passes()
    {
        // Arrange
        var validator = new CreateProtocolTemplateCommandValidator();
        var command = ValidCreateCommand();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // --- UpdateProtocolTemplate Tests ---

    [Fact]
    public async Task UpdateProtocolTemplate_WithValidData_ReturnsSuccessWithDto()
    {
        // Arrange
        SetupValidUpdateValidator();
        var protocolId = Guid.NewGuid();
        var existingProtocol = MakeProtocol();
        _repository.GetByIdAsync(protocolId, Arg.Any<CancellationToken>())
            .Returns(existingProtocol);

        var command = ValidUpdateCommand(protocolId);

        // Act
        var result = await UpdateProtocolTemplateHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated IPL 6-session");
        result.Value.DefaultSessionCount.Should().Be(6);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProtocolTemplate_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        SetupValidUpdateValidator();
        var nonExistentId = Guid.NewGuid();
        _repository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((TreatmentProtocol?)null);

        var command = ValidUpdateCommand(nonExistentId);

        // Act
        var result = await UpdateProtocolTemplateHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task UpdateProtocolTemplate_WithValidationError_ReturnsValidationFailure()
    {
        // Arrange
        var command = ValidUpdateCommand() with { Name = "" };
        _updateValidator.ValidateAsync(Arg.Any<UpdateProtocolTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Name is required.")
            }));

        // Act
        var result = await UpdateProtocolTemplateHandler.Handle(
            command, _repository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // --- GetProtocolTemplates Tests ---

    [Fact]
    public async Task GetProtocolTemplates_ReturnsListOfDtos()
    {
        // Arrange
        var protocols = new List<TreatmentProtocol> { MakeProtocol(), MakeProtocol() };
        _repository.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(protocols);

        var query = new GetProtocolTemplatesQuery();

        // Act
        var result = await GetProtocolTemplatesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProtocolTemplates_WithIncludeInactive_PassesFlagToRepository()
    {
        // Arrange
        _repository.GetAllAsync(true, Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentProtocol>());

        var query = new GetProtocolTemplatesQuery(IncludeInactive: true);

        // Act
        var result = await GetProtocolTemplatesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetAllAsync(true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProtocolTemplates_WithTypeFilter_CallsGetByType()
    {
        // Arrange
        _repository.GetByTypeAsync(TreatmentType.IPL, false, Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentProtocol> { MakeProtocol() });

        var query = new GetProtocolTemplatesQuery(TreatmentType: (int)TreatmentType.IPL);

        // Act
        var result = await GetProtocolTemplatesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        await _repository.Received(1).GetByTypeAsync(TreatmentType.IPL, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProtocolTemplates_WhenNoTemplatesExist_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentProtocol>());

        var query = new GetProtocolTemplatesQuery();

        // Act
        var result = await GetProtocolTemplatesHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // --- GetProtocolTemplateById Tests ---

    [Fact]
    public async Task GetProtocolTemplateById_WithValidId_ReturnsDto()
    {
        // Arrange
        var protocol = MakeProtocol();
        _repository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>())
            .Returns(protocol);

        var query = new GetProtocolTemplateByIdQuery(protocol.Id);

        // Act
        var result = await GetProtocolTemplateByIdHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(protocol.Id);
        result.Value.Name.Should().Be(protocol.Name);
        result.Value.DefaultSessionCount.Should().Be(protocol.DefaultSessionCount);
    }

    [Fact]
    public async Task GetProtocolTemplateById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((TreatmentProtocol?)null);

        var query = new GetProtocolTemplateByIdQuery(nonExistentId);

        // Act
        var result = await GetProtocolTemplateByIdHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    // --- DTO Mapping Tests ---

    [Fact]
    public async Task CreateProtocolTemplate_MapsAllFieldsToDto()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = ValidCreateCommand();

        // Act
        var result = await CreateProtocolTemplateHandler.Handle(
            command, _repository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.Name.Should().Be(command.Name);
        dto.TreatmentType.Should().Be(TreatmentType.IPL.ToString());
        dto.DefaultSessionCount.Should().Be(command.DefaultSessionCount);
        dto.PricingMode.Should().Be(PricingMode.PerPackage.ToString());
        dto.DefaultPackagePrice.Should().Be(command.DefaultPackagePrice);
        dto.DefaultSessionPrice.Should().Be(command.DefaultSessionPrice);
        dto.MinIntervalDays.Should().Be(command.MinIntervalDays);
        dto.MaxIntervalDays.Should().Be(command.MaxIntervalDays);
        dto.CancellationDeductionPercent.Should().Be(command.CancellationDeductionPercent);
        dto.IsActive.Should().BeTrue();
        dto.Description.Should().Be(command.Description);
    }
}
