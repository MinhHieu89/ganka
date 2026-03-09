using FluentAssertions;
using NSubstitute;
using Scheduling.Application.Features;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Unit.Tests.Features;

/// <summary>
/// Tests for GetAppointmentsByDoctorHandler verifying that both Name and NameVi
/// from AppointmentType are mapped into AppointmentDto.
/// </summary>
public class GetAppointmentsByDoctorHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();

    private static Appointment CreateAppointment(Guid appointmentTypeId, DateTimeKind kind = DateTimeKind.Utc)
    {
        return Appointment.Create(
            patientId: Guid.NewGuid(),
            patientName: "Test Patient",
            doctorId: Guid.NewGuid(),
            doctorName: "Dr. Test",
            startTime: new DateTime(2026, 3, 10, 9, 0, 0, kind),
            endTime: new DateTime(2026, 3, 10, 9, 30, 0, kind),
            appointmentTypeId: appointmentTypeId,
            branchId: new Shared.Domain.BranchId(Guid.NewGuid()),
            notes: "Test note");
    }

    [Fact]
    public async Task Handle_MapsAppointmentTypeNameVi_FromAppointmentType()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var dateFrom = new DateTime(2026, 3, 10);
        var dateTo = new DateTime(2026, 3, 11);
        var query = new GetAppointmentsByDoctorQuery(doctorId, dateFrom, dateTo);

        var appointmentType = new AppointmentType(Guid.NewGuid(), "Follow Up", "Tai kham", 30, "#4CAF50");
        var appointment = CreateAppointment(appointmentType.Id);

        _appointmentRepository.GetByDoctorAsync(doctorId, dateFrom, dateTo, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment });

        _appointmentRepository.GetAllAppointmentTypesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AppointmentType> { appointmentType });

        // Act
        var result = await GetAppointmentsByDoctorHandler.Handle(query, _appointmentRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].AppointmentTypeName.Should().Be("Follow Up");
        result[0].AppointmentTypeNameVi.Should().Be("Tai kham");
    }

    [Fact]
    public async Task Handle_ReturnsUnknown_ForAppointmentTypeNameVi_WhenTypeNotFound()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var dateFrom = new DateTime(2026, 3, 10);
        var dateTo = new DateTime(2026, 3, 11);
        var query = new GetAppointmentsByDoctorQuery(doctorId, dateFrom, dateTo);

        var appointment = CreateAppointment(Guid.NewGuid()); // Non-existent type ID

        _appointmentRepository.GetByDoctorAsync(doctorId, dateFrom, dateTo, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment });

        _appointmentRepository.GetAllAppointmentTypesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AppointmentType>()); // Empty - type not found

        // Act
        var result = await GetAppointmentsByDoctorHandler.Handle(query, _appointmentRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].AppointmentTypeName.Should().Be("Unknown");
        result[0].AppointmentTypeNameVi.Should().Be("Unknown");
    }

    [Fact]
    public async Task Handle_ReturnsUtcDateTimeKind_ForStartTimeAndEndTime()
    {
        // Arrange - simulate EF Core returning DateTimeKind.Unspecified (its default behavior)
        var doctorId = Guid.NewGuid();
        var dateFrom = new DateTime(2026, 3, 10);
        var dateTo = new DateTime(2026, 3, 11);
        var query = new GetAppointmentsByDoctorQuery(doctorId, dateFrom, dateTo);

        var appointmentType = new AppointmentType(Guid.NewGuid(), "Follow Up", "Tai kham", 30, "#4CAF50");
        var appointment = CreateAppointment(appointmentType.Id, DateTimeKind.Unspecified);

        _appointmentRepository.GetByDoctorAsync(doctorId, dateFrom, dateTo, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment });

        _appointmentRepository.GetAllAppointmentTypesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AppointmentType> { appointmentType });

        // Act
        var result = await GetAppointmentsByDoctorHandler.Handle(query, _appointmentRepository, CancellationToken.None);

        // Assert - DTO must have DateTimeKind.Utc so JSON serializer adds 'Z' suffix
        result.Should().HaveCount(1);
        result[0].StartTime.Kind.Should().Be(DateTimeKind.Utc);
        result[0].EndTime.Kind.Should().Be(DateTimeKind.Utc);
    }
}
