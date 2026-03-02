using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Scheduling.Application.Features;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Entities;
using Shared.Domain;

namespace Scheduling.Unit.Tests.Features;

/// <summary>
/// Tests for BookAppointmentHandler verifying UTC-to-Vietnam timezone conversion
/// before clinic schedule validation.
/// </summary>
public class BookAppointmentHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IClinicScheduleRepository _clinicScheduleRepository = Substitute.For<IClinicScheduleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<BookAppointmentCommand> _validator = Substitute.For<IValidator<BookAppointmentCommand>>();
    private readonly ILogger<BookAppointmentCommand> _logger = Substitute.For<ILogger<BookAppointmentCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<BookAppointmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private AppointmentType CreateAppointmentType(int durationMinutes = 30)
    {
        return new AppointmentType(Guid.NewGuid(), "FollowUp", "Tai kham", durationMinutes, "#4CAF50");
    }

    [Fact]
    public async Task Handle_UtcMorning_VietnamAfternoon_WithinSchedule_Succeeds()
    {
        // Arrange
        // UTC 07:00 = 14:00 Vietnam (UTC+7)
        // Schedule: 13:00-20:00 on that day -> should PASS
        SetupValidValidator();
        var appointmentType = CreateAppointmentType(30);
        var appointmentTypeId = appointmentType.Id;

        // Use a future Monday in UTC. UTC 07:00 Monday = 14:00 Monday Vietnam
        var utcStart = new DateTime(2026, 3, 9, 7, 0, 0, DateTimeKind.Utc); // Monday in both UTC and Vietnam

        var command = new BookAppointmentCommand(
            PatientId: Guid.NewGuid(),
            PatientName: "Test Patient",
            DoctorId: Guid.NewGuid(),
            DoctorName: "Dr. Test",
            StartTime: utcStart,
            AppointmentTypeId: appointmentTypeId,
            Notes: null);

        _appointmentRepository.GetAppointmentTypeAsync(appointmentTypeId, Arg.Any<CancellationToken>())
            .Returns(appointmentType);

        // Vietnam Monday schedule: 13:00-20:00
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var schedule = new ClinicSchedule(DayOfWeek.Monday, true,
            new TimeSpan(13, 0, 0), new TimeSpan(20, 0, 0), branchId);
        _clinicScheduleRepository.GetForDayAsync(DayOfWeek.Monday, Arg.Any<CancellationToken>())
            .Returns(schedule);

        _appointmentRepository.HasOverlappingAsync(
            command.DoctorId, command.StartTime, Arg.Any<DateTime>(), ct: Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await BookAppointmentHandler.Handle(
            command, _appointmentRepository, _clinicScheduleRepository, _unitOfWork, _validator, _logger, CancellationToken.None);

        // Assert - should succeed because 14:00-14:30 Vietnam is within 13:00-20:00
        result.IsSuccess.Should().BeTrue("UTC 07:00 = 14:00 Vietnam which is within schedule 13:00-20:00");
    }

    [Fact]
    public async Task Handle_UtcLateNight_VietnamEarlyMorning_OutsideSchedule_Fails()
    {
        // Arrange
        // UTC 23:00 = 06:00 Vietnam next day
        // Schedule: 08:00-20:00 -> 06:00 is OUTSIDE hours
        SetupValidValidator();
        var appointmentType = CreateAppointmentType(30);
        var appointmentTypeId = appointmentType.Id;

        // UTC 23:00 Sunday = 06:00 Monday Vietnam
        var utcStart = new DateTime(2026, 3, 8, 23, 0, 0, DateTimeKind.Utc); // Sunday UTC = Monday Vietnam

        var command = new BookAppointmentCommand(
            PatientId: Guid.NewGuid(),
            PatientName: "Test Patient",
            DoctorId: Guid.NewGuid(),
            DoctorName: "Dr. Test",
            StartTime: utcStart,
            AppointmentTypeId: appointmentTypeId,
            Notes: null);

        _appointmentRepository.GetAppointmentTypeAsync(appointmentTypeId, Arg.Any<CancellationToken>())
            .Returns(appointmentType);

        // Vietnam Monday schedule: 08:00-20:00 (the converted day)
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var mondaySchedule = new ClinicSchedule(DayOfWeek.Monday, true,
            new TimeSpan(8, 0, 0), new TimeSpan(20, 0, 0), branchId);
        _clinicScheduleRepository.GetForDayAsync(DayOfWeek.Monday, Arg.Any<CancellationToken>())
            .Returns(mondaySchedule);

        // Also set Sunday as closed (in case handler uses UTC day directly)
        _clinicScheduleRepository.GetForDayAsync(DayOfWeek.Sunday, Arg.Any<CancellationToken>())
            .Returns((ClinicSchedule?)null);

        // Act
        var result = await BookAppointmentHandler.Handle(
            command, _appointmentRepository, _clinicScheduleRepository, _unitOfWork, _validator, _logger, CancellationToken.None);

        // Assert - should fail because 06:00 Vietnam is outside 08:00-20:00
        result.IsFailure.Should().BeTrue("UTC 23:00 = 06:00 Vietnam which is outside schedule 08:00-20:00");
        result.Error.Code.Should().Be("Error.Validation");
    }
}
