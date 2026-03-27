using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Patient.Application.Interfaces;
using Patient.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class CreateWalkInVisitTests
{
    private readonly IVisitRepository _visitRepo = Substitute.For<IVisitRepository>();
    private readonly IPatientRepository _patientRepo = Substitute.For<IPatientRepository>();
    private readonly Clinical.Application.Interfaces.IUnitOfWork _unitOfWork = Substitute.For<Clinical.Application.Interfaces.IUnitOfWork>();
    private readonly IValidator<CreateWalkInVisitCommand> _validator = Substitute.For<IValidator<CreateWalkInVisitCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public CreateWalkInVisitTests()
    {
        _currentUser.BranchId.Returns(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    }

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<CreateWalkInVisitCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private Patient.Domain.Entities.Patient CreateTestPatient()
    {
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        return Patient.Domain.Entities.Patient.Create(
            "Test Patient", "0901234567", PatientType.Medical,
            branchId, DateTime.UtcNow.AddYears(-30), Gender.Male);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesWalkInVisit()
    {
        // Arrange
        SetupValidValidator();
        var patient = CreateTestPatient();
        var command = new CreateWalkInVisitCommand(patient.Id, Guid.NewGuid(), "Dr. Test", "Eye pain");

        _patientRepo.GetByIdAsync(patient.Id, Arg.Any<CancellationToken>())
            .Returns(patient);

        // Act
        var result = await CreateWalkInVisitHandler.Handle(
            command, _visitRepo, _patientRepo, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _visitRepo.Received(1).AddAsync(
            Arg.Is<Clinical.Domain.Entities.Visit>(v =>
                v.Source == VisitSource.WalkIn &&
                v.AppointmentId == null &&
                v.Reason == "Eye pain"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PatientNotFound_ReturnsNotFound()
    {
        // Arrange
        SetupValidValidator();
        var command = new CreateWalkInVisitCommand(Guid.NewGuid(), Guid.NewGuid(), "Dr. Test", null);

        _patientRepo.GetByIdAsync(command.PatientId, Arg.Any<CancellationToken>())
            .Returns((Patient.Domain.Entities.Patient?)null);

        // Act
        var result = await CreateWalkInVisitHandler.Handle(
            command, _visitRepo, _patientRepo, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
