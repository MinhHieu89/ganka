using FluentAssertions;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Unit.Tests.Domain;

/// <summary>
/// Tests for receptionist-related Appointment domain extensions:
/// guest bookings, check-in, no-show marking, and AppointmentSource.
/// </summary>
public class AppointmentReceptionistTests
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    private static readonly Guid DoctorId = Guid.NewGuid();
    private static readonly Guid AppointmentTypeId = Guid.NewGuid();

    private static Appointment CreateConfirmedAppointment()
    {
        return Appointment.Create(
            Guid.NewGuid(), "Test Patient", DoctorId, "Dr. Test",
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2),
            AppointmentTypeId, DefaultBranchId);
    }

    // ==================== Guest Booking Tests ====================

    [Fact]
    public void CreateGuest_SetsGuestFieldsAndNullPatientId()
    {
        // Act
        var appointment = Appointment.CreateGuest(
            "Nguyen Van A", "0901234567", "Kham mat thuong quy",
            DoctorId, "Dr. Test",
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2),
            AppointmentTypeId, DefaultBranchId, AppointmentSource.Phone);

        // Assert
        appointment.PatientId.Should().BeNull();
        appointment.GuestName.Should().Be("Nguyen Van A");
        appointment.GuestPhone.Should().Be("0901234567");
        appointment.GuestReason.Should().Be("Kham mat thuong quy");
        appointment.Source.Should().Be(AppointmentSource.Phone);
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void CreateGuest_WithNullReason_IsValid()
    {
        var appointment = Appointment.CreateGuest(
            "Guest", "0901234567", null,
            DoctorId, "Dr. Test",
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2),
            AppointmentTypeId, DefaultBranchId, AppointmentSource.Staff);

        appointment.GuestReason.Should().BeNull();
        appointment.PatientId.Should().BeNull();
    }

    [Fact]
    public void Create_WithSource_SetsSource()
    {
        var appointment = Appointment.Create(
            Guid.NewGuid(), "Patient", DoctorId, "Dr. Test",
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2),
            AppointmentTypeId, DefaultBranchId,
            source: AppointmentSource.Web);

        appointment.Source.Should().Be(AppointmentSource.Web);
    }

    [Fact]
    public void Create_DefaultSource_IsStaff()
    {
        var appointment = CreateConfirmedAppointment();
        appointment.Source.Should().Be(AppointmentSource.Staff);
    }

    // ==================== Check-In Tests ====================

    [Fact]
    public void CheckIn_Confirmed_SetsCheckedInAt()
    {
        var appointment = CreateConfirmedAppointment();

        appointment.CheckIn();

        appointment.CheckedInAt.Should().NotBeNull();
        appointment.CheckedInAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CheckIn_AlreadyCheckedIn_Throws()
    {
        var appointment = CreateConfirmedAppointment();
        appointment.CheckIn();

        var act = () => appointment.CheckIn();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CheckIn_Cancelled_Throws()
    {
        var appointment = CreateConfirmedAppointment();
        appointment.Cancel(CancellationReason.PatientRequest, null);

        var act = () => appointment.CheckIn();

        act.Should().Throw<InvalidOperationException>();
    }

    // ==================== No-Show Tests ====================

    [Fact]
    public void MarkNoShow_Confirmed_SetsNoShowFields()
    {
        var appointment = CreateConfirmedAppointment();
        var userId = Guid.NewGuid();

        appointment.MarkNoShow(userId, "BN khong den");

        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        appointment.NoShowAt.Should().NotBeNull();
        appointment.NoShowBy.Should().Be(userId);
        appointment.NoShowNotes.Should().Be("BN khong den");
    }

    [Fact]
    public void MarkNoShow_Cancelled_Throws()
    {
        var appointment = CreateConfirmedAppointment();
        appointment.Cancel(CancellationReason.PatientRequest, null);

        var act = () => appointment.MarkNoShow(Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkNoShow_Completed_Throws()
    {
        var appointment = CreateConfirmedAppointment();
        appointment.Complete();

        var act = () => appointment.MarkNoShow(Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkNoShow_AlreadyNoShow_Throws()
    {
        var appointment = CreateConfirmedAppointment();
        appointment.MarkNoShow(Guid.NewGuid(), null);

        var act = () => appointment.MarkNoShow(Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>();
    }

    // ==================== AppointmentStatus NoShow Value ====================

    [Fact]
    public void AppointmentStatus_NoShow_HasValue4()
    {
        ((int)AppointmentStatus.NoShow).Should().Be(4);
    }
}
