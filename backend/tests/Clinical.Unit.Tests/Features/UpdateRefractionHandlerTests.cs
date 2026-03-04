using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class UpdateRefractionHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<UpdateRefractionCommand> _validator = Substitute.For<IValidator<UpdateRefractionCommand>>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<UpdateRefractionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static Visit CreateEditableVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
    }

    [Fact]
    public async Task Handle_ManifestRefraction_StoresOnVisit()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        var command = new UpdateRefractionCommand(
            visit.Id, (int)RefractionType.Manifest,
            -2.0m, -1.0m, 90m, 1.5m, 32m,
            -1.5m, -0.5m, 180m, 1.25m, 31m,
            0.3m, 0.4m, 1.0m, 0.9m,
            15m, 16m, (int)IopMethod.Goldmann,
            24.5m, 24.3m);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await UpdateRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Refractions.Should().HaveCount(1);
        var refraction = visit.Refractions.First();
        refraction.Type.Should().Be(RefractionType.Manifest);
        refraction.OdSph.Should().Be(-2.0m);
        refraction.OsSph.Should().Be(-1.5m);
    }

    [Fact]
    public async Task Handle_AutorefractionData_CreatesAutoRefractionRecord()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        var command = new UpdateRefractionCommand(
            visit.Id, (int)RefractionType.Autorefraction,
            -3.0m, -1.5m, 45m, null, null,
            -2.5m, -1.0m, 135m, null, null,
            null, null, null, null,
            null, null, null,
            null, null);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await UpdateRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Refractions.Should().HaveCount(1);
        visit.Refractions.First().Type.Should().Be(RefractionType.Autorefraction);
    }

    [Fact]
    public async Task Handle_SignedVisit_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        visit.SignOff(Guid.NewGuid()); // Make it signed
        var command = new UpdateRefractionCommand(
            visit.Id, (int)RefractionType.Manifest,
            -2.0m, null, null, null, null,
            null, null, null, null, null,
            null, null, null, null,
            null, null, null,
            null, null);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await UpdateRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_SphOutOfRange_ReturnsValidationError()
    {
        // Arrange
        var command = new UpdateRefractionCommand(
            Guid.NewGuid(), (int)RefractionType.Manifest,
            -35m, null, null, null, null,  // SPH out of range (-30 to +30)
            null, null, null, null, null,
            null, null, null, null,
            null, null, null,
            null, null);

        _validator.ValidateAsync(Arg.Any<UpdateRefractionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("OdSph", "SPH must be between -30 and +30.")
            }));

        // Act
        var result = await UpdateRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_VaOutOfRange_ReturnsValidationError()
    {
        // Arrange
        var command = new UpdateRefractionCommand(
            Guid.NewGuid(), (int)RefractionType.Manifest,
            null, null, null, null, null,
            null, null, null, null, null,
            3.0m, null, null, null,  // VA out of range (0.01 to 2.0)
            null, null, null,
            null, null);

        _validator.ValidateAsync(Arg.Any<UpdateRefractionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("UcvaOd", "VA must be between 0.01 and 2.0.")
            }));

        // Act
        var result = await UpdateRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidValidator();
        var command = new UpdateRefractionCommand(
            Guid.NewGuid(), (int)RefractionType.Manifest,
            -2.0m, null, null, null, null,
            null, null, null, null, null,
            null, null, null, null,
            null, null, null,
            null, null);

        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await UpdateRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
