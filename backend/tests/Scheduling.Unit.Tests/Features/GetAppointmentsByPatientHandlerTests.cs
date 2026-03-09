using FluentAssertions;
using NSubstitute;
using Scheduling.Application.Features;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Unit.Tests.Features;

/// <summary>
/// Tests for GetAppointmentsByPatientHandler verifying that both Name and NameVi
/// from AppointmentType are mapped into AppointmentDto.
/// </summary>
public class GetAppointmentsByPatientHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();

    private static Appointment CreateAppointment(Guid appointmentTypeId)
    {
        return Appointment.Create(
            patientId: Guid.NewGuid(),
            patientName: "Test Patient",
            doctorId: Guid.NewGuid(),
            doctorName: "Dr. Test",
            startTime: new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc),
            endTime: new DateTime(2026, 3, 10, 9, 30, 0, DateTimeKind.Utc),
            appointmentTypeId: appointmentTypeId,
            branchId: new Shared.Domain.BranchId(Guid.NewGuid()),
            notes: "Test note");
    }

    [Fact]
    public async Task Handle_MapsAppointmentTypeNameVi_FromAppointmentType()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetAppointmentsByPatientQuery(patientId);

        var appointmentType = new AppointmentType(Guid.NewGuid(), "New Patient", "Benh nhan moi", 45, "#3b82f6");
        var appointment = CreateAppointment(appointmentType.Id);

        _appointmentRepository.GetByPatientAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment });

        _appointmentRepository.GetAllAppointmentTypesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AppointmentType> { appointmentType });

        // Act
        var result = await GetAppointmentsByPatientHandler.Handle(query, _appointmentRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].AppointmentTypeName.Should().Be("New Patient");
        result[0].AppointmentTypeNameVi.Should().Be("Benh nhan moi");
    }

    [Fact]
    public async Task Handle_ReturnsUnknown_ForAppointmentTypeNameVi_WhenTypeNotFound()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetAppointmentsByPatientQuery(patientId);

        var appointment = CreateAppointment(Guid.NewGuid()); // Non-existent type ID

        _appointmentRepository.GetByPatientAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment });

        _appointmentRepository.GetAllAppointmentTypesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AppointmentType>()); // Empty - type not found

        // Act
        var result = await GetAppointmentsByPatientHandler.Handle(query, _appointmentRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].AppointmentTypeName.Should().Be("Unknown");
        result[0].AppointmentTypeNameVi.Should().Be("Unknown");
    }
}
