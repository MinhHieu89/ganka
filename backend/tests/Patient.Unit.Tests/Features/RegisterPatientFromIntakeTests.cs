using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Patient.Application.Features;
using Patient.Application.Interfaces;
using Patient.Contracts.Dtos;
using Patient.Contracts.Enums;
using Shared.Domain;

namespace Patient.Unit.Tests.Features;

public class RegisterPatientFromIntakeTests
{
    private readonly IPatientRepository _patientRepo = Substitute.For<IPatientRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<RegisterPatientFromIntakeCommand> _validator = Substitute.For<IValidator<RegisterPatientFromIntakeCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<RegisterPatientFromIntakeCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesPatientWithIntakeFields()
    {
        // Arrange
        SetupValidValidator();
        _patientRepo.PhoneExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _patientRepo.GetMaxSequenceNumberForYearAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(5);

        var command = new RegisterPatientFromIntakeCommand(
            FullName: "Nguyen Van A",
            Phone: "0901234567",
            DateOfBirth: new DateTime(1990, 5, 15),
            Gender: Gender.Male,
            Address: "123 Hanoi",
            Cccd: "012345678901",
            Email: "test@test.com",
            Occupation: "Engineer",
            OcularHistory: "Near-sighted since age 12",
            SystemicHistory: "None",
            CurrentMedications: "None",
            ScreenTimeHours: 8.5m,
            WorkEnvironment: "Office",
            ContactLensUsage: "Daily",
            LifestyleNotes: "Regular exercise",
            Allergies: null);

        // Act
        var result = await RegisterPatientFromIntakeHandler.Handle(
            command, _patientRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _patientRepo.Received(1).Add(Arg.Is<Domain.Entities.Patient>(p =>
            p.FullName == "Nguyen Van A" &&
            p.Email == "test@test.com" &&
            p.Occupation == "Engineer"));
    }

    [Fact]
    public async Task Handle_GeneratesPatientCode()
    {
        // Arrange
        SetupValidValidator();
        _patientRepo.PhoneExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _patientRepo.GetMaxSequenceNumberForYearAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(42);

        var command = new RegisterPatientFromIntakeCommand(
            FullName: "Test Patient",
            Phone: "0901234568",
            DateOfBirth: new DateTime(1985, 1, 1),
            Gender: Gender.Female,
            Address: null, Cccd: null, Email: null, Occupation: null,
            OcularHistory: null, SystemicHistory: null, CurrentMedications: null,
            ScreenTimeHours: null, WorkEnvironment: null, ContactLensUsage: null,
            LifestyleNotes: null, Allergies: null);

        // Act
        var result = await RegisterPatientFromIntakeHandler.Handle(
            command, _patientRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // SaveChangesAsync should be called twice: first for create, then for code generation
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAllergies_AddsAllergiesToPatient()
    {
        // Arrange
        SetupValidValidator();
        _patientRepo.PhoneExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _patientRepo.GetMaxSequenceNumberForYearAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);

        var command = new RegisterPatientFromIntakeCommand(
            FullName: "Allergy Patient",
            Phone: "0901234569",
            DateOfBirth: new DateTime(2000, 6, 15),
            Gender: Gender.Male,
            Address: null, Cccd: null, Email: null, Occupation: null,
            OcularHistory: null, SystemicHistory: null, CurrentMedications: null,
            ScreenTimeHours: null, WorkEnvironment: null, ContactLensUsage: null,
            LifestyleNotes: null,
            Allergies: [
                new AllergyInput("Penicillin", AllergySeverity.Severe),
                new AllergyInput("Dust", AllergySeverity.Mild)
            ]);

        // Act
        var result = await RegisterPatientFromIntakeHandler.Handle(
            command, _patientRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _patientRepo.Received(1).Add(Arg.Is<Domain.Entities.Patient>(p =>
            p.Allergies.Count == 2));
    }

    [Fact]
    public async Task Handle_DuplicatePhone_ReturnsValidationError()
    {
        // Arrange
        SetupValidValidator();
        _patientRepo.PhoneExistsAsync("0901234567", Arg.Any<CancellationToken>()).Returns(true);

        var command = new RegisterPatientFromIntakeCommand(
            FullName: "Duplicate Phone",
            Phone: "0901234567",
            DateOfBirth: new DateTime(1990, 1, 1),
            Gender: Gender.Male,
            Address: null, Cccd: null, Email: null, Occupation: null,
            OcularHistory: null, SystemicHistory: null, CurrentMedications: null,
            ScreenTimeHours: null, WorkEnvironment: null, ContactLensUsage: null,
            LifestyleNotes: null, Allergies: null);

        // Act
        var result = await RegisterPatientFromIntakeHandler.Handle(
            command, _patientRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }
}

public class UpdatePatientFromIntakeTests
{
    private readonly IPatientRepository _patientRepo = Substitute.For<IPatientRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<UpdatePatientFromIntakeCommand> _validator = Substitute.For<IValidator<UpdatePatientFromIntakeCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<UpdatePatientFromIntakeCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private Domain.Entities.Patient CreateTestPatient()
    {
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        return Domain.Entities.Patient.Create(
            "Original Name", "0901234567", Domain.Enums.PatientType.Medical,
            branchId, DateTime.UtcNow.AddYears(-30), Domain.Enums.Gender.Male);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesIntakeFields()
    {
        // Arrange
        SetupValidValidator();
        var patient = CreateTestPatient();

        _patientRepo.GetByIdWithTrackingAsync(patient.Id, Arg.Any<CancellationToken>())
            .Returns(patient);

        var command = new UpdatePatientFromIntakeCommand(
            PatientId: patient.Id,
            FullName: "Updated Name",
            Phone: "0901234567",
            DateOfBirth: new DateTime(1990, 5, 15),
            Gender: Gender.Male,
            Address: "New Address",
            Cccd: null,
            Email: "updated@test.com",
            Occupation: "Teacher",
            OcularHistory: "Updated history",
            SystemicHistory: null,
            CurrentMedications: null,
            ScreenTimeHours: 6.0m,
            WorkEnvironment: "Office",
            ContactLensUsage: null,
            LifestyleNotes: null,
            Allergies: null);

        // Act
        var result = await UpdatePatientFromIntakeHandler.Handle(
            command, _patientRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        patient.FullName.Should().Be("Updated Name");
        patient.Email.Should().Be("updated@test.com");
        patient.Occupation.Should().Be("Teacher");
    }

    [Fact]
    public async Task Handle_NotFoundPatient_ReturnsNotFound()
    {
        // Arrange
        SetupValidValidator();
        var command = new UpdatePatientFromIntakeCommand(
            PatientId: Guid.NewGuid(),
            FullName: "Test",
            Phone: "0901234567",
            DateOfBirth: null, Gender: null,
            Address: null, Cccd: null, Email: null, Occupation: null,
            OcularHistory: null, SystemicHistory: null, CurrentMedications: null,
            ScreenTimeHours: null, WorkEnvironment: null, ContactLensUsage: null,
            LifestyleNotes: null, Allergies: null);

        _patientRepo.GetByIdWithTrackingAsync(command.PatientId, Arg.Any<CancellationToken>())
            .Returns((Domain.Entities.Patient?)null);

        // Act
        var result = await UpdatePatientFromIntakeHandler.Handle(
            command, _patientRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
