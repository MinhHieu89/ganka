using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class CreateVisitHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateVisitCommand> _validator = Substitute.For<IValidator<CreateVisitCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public CreateVisitHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<CreateVisitCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private CreateVisitCommand CreateValidCommand(Guid? appointmentId = null)
    {
        return new CreateVisitCommand(
            PatientId: Guid.NewGuid(),
            PatientName: "Nguyen Van A",
            DoctorId: Guid.NewGuid(),
            DoctorName: "Dr. Tran",
            HasAllergies: false,
            AppointmentId: appointmentId);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithVisitId()
    {
        // Arrange
        SetupValidValidator();
        var command = CreateValidCommand();

        // Act
        var result = await CreateVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _visitRepository.Received(1).AddAsync(Arg.Any<Visit>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAppointmentId_VisitHasAppointmentIdSet()
    {
        // Arrange
        SetupValidValidator();
        var appointmentId = Guid.NewGuid();
        var command = CreateValidCommand(appointmentId);
        Visit? capturedVisit = null;
        await _visitRepository.AddAsync(Arg.Do<Visit>(v => capturedVisit = v), Arg.Any<CancellationToken>());

        // Act
        var result = await CreateVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedVisit.Should().NotBeNull();
        capturedVisit!.AppointmentId.Should().Be(appointmentId);
    }

    [Fact]
    public async Task Handle_WithoutAppointmentId_VisitHasNullAppointmentId()
    {
        // Arrange
        SetupValidValidator();
        var command = CreateValidCommand();
        Visit? capturedVisit = null;
        await _visitRepository.AddAsync(Arg.Do<Visit>(v => capturedVisit = v), Arg.Any<CancellationToken>());

        // Act
        var result = await CreateVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedVisit.Should().NotBeNull();
        capturedVisit!.AppointmentId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EmptyPatientId_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateVisitCommand(
            PatientId: Guid.Empty,
            PatientName: "Test",
            DoctorId: Guid.NewGuid(),
            DoctorName: "Dr. Test",
            HasAllergies: false,
            AppointmentId: null);

        _validator.ValidateAsync(Arg.Any<CreateVisitCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("PatientId", "Patient is required.")
            }));

        // Act
        var result = await CreateVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_EmptyDoctorName_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateVisitCommand(
            PatientId: Guid.NewGuid(),
            PatientName: "Test",
            DoctorId: Guid.NewGuid(),
            DoctorName: "",
            HasAllergies: false,
            AppointmentId: null);

        _validator.ValidateAsync(Arg.Any<CreateVisitCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("DoctorName", "Doctor name is required.")
            }));

        // Act
        var result = await CreateVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_ValidCommand_VisitHasCorrectInitialState()
    {
        // Arrange
        SetupValidValidator();
        var command = CreateValidCommand();
        Visit? capturedVisit = null;
        await _visitRepository.AddAsync(Arg.Do<Visit>(v => capturedVisit = v), Arg.Any<CancellationToken>());

        // Act
        await CreateVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        capturedVisit.Should().NotBeNull();
        capturedVisit!.CurrentStage.Should().Be(WorkflowStage.Reception);
        capturedVisit.Status.Should().Be(VisitStatus.Draft);
        capturedVisit.PatientName.Should().Be(command.PatientName);
        capturedVisit.DoctorName.Should().Be(command.DoctorName);
    }
}
