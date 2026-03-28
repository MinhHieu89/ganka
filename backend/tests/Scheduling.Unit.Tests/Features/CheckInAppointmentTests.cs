using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Scheduling.Application.Features;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Unit.Tests.Features;

public class CheckInAppointmentTests
{
    private readonly IAppointmentRepository _appointmentRepo = Substitute.For<IAppointmentRepository>();
    private readonly Scheduling.Application.Interfaces.IUnitOfWork _unitOfWork = Substitute.For<Scheduling.Application.Interfaces.IUnitOfWork>();
    private readonly IValidator<CheckInAppointmentCommand> _validator = Substitute.For<IValidator<CheckInAppointmentCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<CheckInAppointmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private Appointment CreateConfirmedAppointment()
    {
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        return Appointment.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(1.5),
            Guid.NewGuid(), branchId);
    }

    [Fact]
    public async Task Handle_ConfirmedAppointment_ReturnsSuccess_SetsCheckedInAt()
    {
        // Arrange
        SetupValidValidator();
        var appointment = CreateConfirmedAppointment();
        var command = new CheckInAppointmentCommand(appointment.Id, Guid.NewGuid());

        _appointmentRepo.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>())
            .Returns(appointment);

        // Act
        var result = await CheckInAppointmentHandler.Handle(
            command, _appointmentRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        appointment.CheckedInAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancelledAppointment_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var appointment = CreateConfirmedAppointment();
        appointment.Cancel(CancellationReason.PatientRequest, "test");

        var command = new CheckInAppointmentCommand(appointment.Id, Guid.NewGuid());

        _appointmentRepo.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>())
            .Returns(appointment);

        // Act
        var result = await CheckInAppointmentHandler.Handle(
            command, _appointmentRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_NotFoundAppointment_ReturnsNotFound()
    {
        // Arrange
        SetupValidValidator();
        var command = new CheckInAppointmentCommand(Guid.NewGuid(), Guid.NewGuid());

        _appointmentRepo.GetByIdAsync(command.AppointmentId, Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);

        // Act
        var result = await CheckInAppointmentHandler.Handle(
            command, _appointmentRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
