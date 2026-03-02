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

/// <summary>
/// Tests for ApproveSelfBookingHandler verifying UTC-to-Vietnam timezone conversion
/// before clinic schedule validation.
/// </summary>
public class ApproveSelfBookingHandlerTests
{
    private readonly ISelfBookingRepository _selfBookingRepository = Substitute.For<ISelfBookingRepository>();
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IClinicScheduleRepository _clinicScheduleRepository = Substitute.For<IClinicScheduleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<ApproveSelfBookingCommand> _validator = Substitute.For<IValidator<ApproveSelfBookingCommand>>();
    private readonly ILogger<ApproveSelfBookingCommand> _logger = Substitute.For<ILogger<ApproveSelfBookingCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<ApproveSelfBookingCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private AppointmentType CreateAppointmentType(int durationMinutes = 30)
    {
        return new AppointmentType(Guid.NewGuid(), "FollowUp", "Tai kham", durationMinutes, "#4CAF50");
    }

    private SelfBookingRequest CreatePendingSelfBooking(Guid appointmentTypeId)
    {
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        return SelfBookingRequest.Create(
            "Test Patient", "0901234567", null, null,
            DateTime.UtcNow.AddDays(7), null, appointmentTypeId, null, branchId);
    }

    [Fact]
    public async Task Handle_UtcMorning_VietnamAfternoon_WithinSchedule_Succeeds()
    {
        // Arrange
        // UTC 07:00 = 14:00 Vietnam (UTC+7)
        // Schedule: 13:00-20:00 -> 14:00 is WITHIN hours
        SetupValidValidator();
        var appointmentType = CreateAppointmentType(30);
        var selfBooking = CreatePendingSelfBooking(appointmentType.Id);

        // UTC 07:00 Monday = 14:00 Monday Vietnam
        var utcStart = new DateTime(2026, 3, 9, 7, 0, 0, DateTimeKind.Utc); // Monday

        var command = new ApproveSelfBookingCommand(
            SelfBookingRequestId: selfBooking.Id,
            DoctorId: Guid.NewGuid(),
            DoctorName: "Dr. Test",
            PatientName: "Test Patient",
            StartTime: utcStart);

        _selfBookingRepository.GetByIdAsync(selfBooking.Id, Arg.Any<CancellationToken>())
            .Returns(selfBooking);
        _appointmentRepository.GetAppointmentTypeAsync(selfBooking.AppointmentTypeId, Arg.Any<CancellationToken>())
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
        var result = await ApproveSelfBookingHandler.Handle(
            command, _selfBookingRepository, _appointmentRepository, _clinicScheduleRepository,
            _unitOfWork, _validator, _logger, CancellationToken.None);

        // Assert - should succeed because 14:00-14:30 Vietnam is within 13:00-20:00
        result.IsSuccess.Should().BeTrue("UTC 07:00 = 14:00 Vietnam which is within schedule 13:00-20:00");
    }
}
