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

public class MarkNoShowTests
{
    private readonly IAppointmentRepository _appointmentRepo = Substitute.For<IAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<MarkAppointmentNoShowCommand> _validator = Substitute.For<IValidator<MarkAppointmentNoShowCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<MarkAppointmentNoShowCommand>(), Arg.Any<CancellationToken>())
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
    public async Task Handle_ValidAppointment_SetsNoShowStatus()
    {
        // Arrange
        SetupValidValidator();
        var appointment = CreateConfirmedAppointment();
        var userId = Guid.NewGuid();
        var command = new MarkAppointmentNoShowCommand(appointment.Id, userId, "Did not arrive");

        _appointmentRepo.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>())
            .Returns(appointment);

        // Act
        var result = await MarkAppointmentNoShowHandler.Handle(
            command, _appointmentRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        appointment.NoShowBy.Should().Be(userId);
        appointment.NoShowNotes.Should().Be("Did not arrive");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancelledAppointment_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var appointment = CreateConfirmedAppointment();
        appointment.Cancel(CancellationReason.PatientRequest, null);

        var command = new MarkAppointmentNoShowCommand(appointment.Id, Guid.NewGuid(), null);

        _appointmentRepo.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>())
            .Returns(appointment);

        // Act
        var result = await MarkAppointmentNoShowHandler.Handle(
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
        var command = new MarkAppointmentNoShowCommand(Guid.NewGuid(), Guid.NewGuid(), null);

        _appointmentRepo.GetByIdAsync(command.AppointmentId, Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);

        // Act
        var result = await MarkAppointmentNoShowHandler.Handle(
            command, _appointmentRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
