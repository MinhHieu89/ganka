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

public class UpdateDryEyeAssessmentHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<UpdateDryEyeAssessmentCommand> _validator = Substitute.For<IValidator<UpdateDryEyeAssessmentCommand>>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<UpdateDryEyeAssessmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static Visit CreateEditableVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesNewAssessment()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        var command = new UpdateDryEyeAssessmentCommand(
            visit.Id,
            10.5m, 9.0m,   // TBUT OD/OS
            15m, 12m,       // Schirmer OD/OS
            1, 2,           // Meibomian OD/OS
            0.3m, 0.25m,   // Tear meniscus OD/OS
            2, 3);          // Staining OD/OS

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await UpdateDryEyeAssessmentHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.DryEyeAssessments.Should().HaveCount(1);
        var assessment = visit.DryEyeAssessments.First();
        assessment.OdTbut.Should().Be(10.5m);
        assessment.OsTbut.Should().Be(9.0m);
        assessment.OdSchirmer.Should().Be(15m);
        assessment.OsSchirmer.Should().Be(12m);
        assessment.OdMeibomianGrading.Should().Be(1);
        assessment.OsMeibomianGrading.Should().Be(2);
        assessment.OdTearMeniscus.Should().Be(0.3m);
        assessment.OsTearMeniscus.Should().Be(0.25m);
        assessment.OdStaining.Should().Be(2);
        assessment.OsStaining.Should().Be(3);
        _visitRepository.Received(1).AddDryEyeAssessment(Arg.Any<DryEyeAssessment>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingAssessment_UpdatesIt()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();

        // Create existing assessment on the visit via the first call
        var firstCommand = new UpdateDryEyeAssessmentCommand(
            visit.Id, 5m, 5m, 10m, 10m, 0, 0, 0.2m, 0.2m, 1, 1);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        await UpdateDryEyeAssessmentHandler.Handle(
            firstCommand, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        visit.DryEyeAssessments.Should().HaveCount(1);

        // Act - update the existing assessment
        var updateCommand = new UpdateDryEyeAssessmentCommand(
            visit.Id, 12m, 11m, 18m, 15m, 2, 3, 0.4m, 0.35m, 3, 4);
        var result = await UpdateDryEyeAssessmentHandler.Handle(
            updateCommand, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.DryEyeAssessments.Should().HaveCount(1); // Still 1, updated in place
        var assessment = visit.DryEyeAssessments.First();
        assessment.OdTbut.Should().Be(12m);
        assessment.OsTbut.Should().Be(11m);
        assessment.OdSchirmer.Should().Be(18m);
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidValidator();
        var command = new UpdateDryEyeAssessmentCommand(
            Guid.NewGuid(), 10m, 9m, 15m, 12m, 1, 2, 0.3m, 0.25m, 2, 3);

        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await UpdateDryEyeAssessmentHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_SignedVisit_ReturnsValidationError()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        visit.SignOff(Guid.NewGuid()); // Make it signed

        var command = new UpdateDryEyeAssessmentCommand(
            visit.Id, 10m, 9m, 15m, 12m, 1, 2, 0.3m, 0.25m, 2, 3);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await UpdateDryEyeAssessmentHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_InvalidMeibomianGrading_ReturnsValidationError()
    {
        // Arrange - Meibomian grading out of range (0-3)
        var command = new UpdateDryEyeAssessmentCommand(
            Guid.NewGuid(), null, null, null, null, 5, null, null, null, null, null);

        _validator.ValidateAsync(Arg.Any<UpdateDryEyeAssessmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("OdMeibomianGrading", "Meibomian grading must be between 0 and 3.")
            }));

        // Act
        var result = await UpdateDryEyeAssessmentHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_AllNullFields_CreatesEmptyAssessment()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        var command = new UpdateDryEyeAssessmentCommand(
            visit.Id, null, null, null, null, null, null, null, null, null, null);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await UpdateDryEyeAssessmentHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.DryEyeAssessments.Should().HaveCount(1);
        var assessment = visit.DryEyeAssessments.First();
        assessment.OdTbut.Should().BeNull();
        assessment.OsTbut.Should().BeNull();
    }
}
