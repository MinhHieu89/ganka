using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Scheduling.Application.Features;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Unit.Tests.Features;

public class BookGuestAppointmentTests
{
    private readonly IAppointmentRepository _appointmentRepo = Substitute.For<IAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<BookGuestAppointmentCommand> _validator = Substitute.For<IValidator<BookGuestAppointmentCommand>>();
    private readonly ILogger<BookGuestAppointmentCommand> _logger = Substitute.For<ILogger<BookGuestAppointmentCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<BookGuestAppointmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Fact]
    public async Task Handle_GuestNameAndPhone_CreatesAppointmentWithNullPatientId()
    {
        // Arrange
        SetupValidValidator();
        var doctorId = Guid.NewGuid();
        var command = new BookGuestAppointmentCommand(
            GuestName: "Nguyen Van A",
            GuestPhone: "0901234567",
            GuestReason: "Mat mo",
            DoctorId: doctorId,
            DoctorName: "Dr. Test",
            StartTime: DateTime.UtcNow.AddDays(1),
            Source: (int)AppointmentSource.Phone);

        _appointmentRepo.HasOverlappingAsync(
            doctorId, command.StartTime, Arg.Any<DateTime>(), ct: Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await BookGuestAppointmentHandler.Handle(
            command, _appointmentRepo, _unitOfWork, _validator, _logger, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _appointmentRepo.Received(1).Add(Arg.Is<Appointment>(a =>
            a.GuestName == "Nguyen Van A" &&
            a.GuestPhone == "0901234567" &&
            a.PatientId == null));
    }

    [Fact]
    public async Task Handle_DoctorSelected_ChecksOverlap_D12()
    {
        // Arrange
        SetupValidValidator();
        var doctorId = Guid.NewGuid();
        var command = new BookGuestAppointmentCommand(
            GuestName: "Nguyen Van B",
            GuestPhone: "0901234568",
            GuestReason: null,
            DoctorId: doctorId,
            DoctorName: "Dr. Test",
            StartTime: DateTime.UtcNow.AddDays(1),
            Source: (int)AppointmentSource.Phone);

        _appointmentRepo.HasOverlappingAsync(
            doctorId, command.StartTime, Arg.Any<DateTime>(), ct: Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await BookGuestAppointmentHandler.Handle(
            command, _appointmentRepo, _unitOfWork, _validator, _logger, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }

    [Fact]
    public async Task Handle_NoDoctorSelected_SkipsOverlapCheck_D12()
    {
        // Arrange
        SetupValidValidator();
        var command = new BookGuestAppointmentCommand(
            GuestName: "Nguyen Van C",
            GuestPhone: "0901234569",
            GuestReason: null,
            DoctorId: null,
            DoctorName: null,
            StartTime: DateTime.UtcNow.AddDays(1),
            Source: (int)AppointmentSource.Phone);

        // Act
        var result = await BookGuestAppointmentHandler.Handle(
            command, _appointmentRepo, _unitOfWork, _validator, _logger, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should NOT have called overlap check
        await _appointmentRepo.DidNotReceive().HasOverlappingAsync(
            Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
