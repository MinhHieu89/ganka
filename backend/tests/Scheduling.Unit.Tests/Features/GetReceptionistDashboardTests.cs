using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Scheduling.Application.Features;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Queries;
using Scheduling.Domain.Entities;
using Shared.Domain;

namespace Scheduling.Unit.Tests.Features;

public class GetReceptionistDashboardTests
{
    private readonly IAppointmentRepository _appointmentRepo = Substitute.For<IAppointmentRepository>();
    private readonly IVisitRepository _visitRepo = Substitute.For<IVisitRepository>();

    private static readonly BranchId DefaultBranch = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private Appointment CreateConfirmedAppointment(Guid? patientId = null, string name = "Patient A")
    {
        return Appointment.Create(
            patientId ?? Guid.NewGuid(), name, Guid.NewGuid(), "Dr. Test",
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(1.5),
            Guid.NewGuid(), DefaultBranch);
    }

    private Visit CreateVisit(Guid patientId, string patientName, WorkflowStage stage,
        VisitStatus status = VisitStatus.Draft, Guid? appointmentId = null,
        VisitSource source = VisitSource.Appointment)
    {
        var visit = Visit.Create(
            patientId, patientName, Guid.NewGuid(), "Dr. Test",
            DefaultBranch, false, appointmentId, source);

        // Advance to desired stage if needed
        if (stage > WorkflowStage.Reception)
        {
            // We need to advance through stages
            if (stage >= WorkflowStage.PreExam)
                visit.AdvanceStage(WorkflowStage.PreExam);
            if (stage >= WorkflowStage.DoctorExam)
                visit.AdvanceStage(WorkflowStage.DoctorExam);
        }

        if (status == VisitStatus.Signed)
            visit.SignOff(Guid.NewGuid());

        return visit;
    }

    [Fact]
    public async Task Handle_MapsAppointmentsAndVisitsTo4Statuses()
    {
        // Arrange
        var apt1 = CreateConfirmedAppointment(name: "Not Arrived Patient");
        var apt2 = CreateConfirmedAppointment(name: "Waiting Patient");
        var apt3 = CreateConfirmedAppointment(name: "Examining Patient");

        var visit2 = CreateVisit(apt2.PatientId!.Value, "Waiting Patient", WorkflowStage.Reception,
            appointmentId: apt2.Id);
        var visit3 = CreateVisit(apt3.PatientId!.Value, "Examining Patient", WorkflowStage.DoctorExam,
            appointmentId: apt3.Id);

        _appointmentRepo.GetTodayAppointmentsAsync(Arg.Any<CancellationToken>())
            .Returns([apt1, apt2, apt3]);
        _visitRepo.GetTodayVisitsAsync(Arg.Any<CancellationToken>())
            .Returns([visit2, visit3]);

        var query = new GetReceptionistDashboardQuery(null, null, 1, 50);

        // Act
        var result = await GetReceptionistDashboardHandler.Handle(
            query, _appointmentRepo, _visitRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var items = result.Value.Items;
        items.Should().HaveCount(3);
        items.Should().Contain(r => r.Status == "not_arrived" && r.PatientName == "Not Arrived Patient");
        items.Should().Contain(r => r.Status == "waiting" && r.PatientName == "Waiting Patient");
        items.Should().Contain(r => r.Status == "examining" && r.PatientName == "Examining Patient");
    }

    [Fact]
    public async Task Handle_FiltersByStatus()
    {
        // Arrange
        var apt1 = CreateConfirmedAppointment(name: "Patient A");
        var apt2 = CreateConfirmedAppointment(name: "Patient B");
        var visit2 = CreateVisit(apt2.PatientId!.Value, "Patient B", WorkflowStage.Reception,
            appointmentId: apt2.Id);

        _appointmentRepo.GetTodayAppointmentsAsync(Arg.Any<CancellationToken>())
            .Returns([apt1, apt2]);
        _visitRepo.GetTodayVisitsAsync(Arg.Any<CancellationToken>())
            .Returns([visit2]);

        var query = new GetReceptionistDashboardQuery("waiting", null, 1, 50);

        // Act
        var result = await GetReceptionistDashboardHandler.Handle(
            query, _appointmentRepo, _visitRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Status.Should().Be("waiting");
    }

    [Fact]
    public async Task Handle_SearchesByPatientName()
    {
        // Arrange
        var apt1 = CreateConfirmedAppointment(name: "Nguyen Van A");
        var apt2 = CreateConfirmedAppointment(name: "Tran Thi B");

        _appointmentRepo.GetTodayAppointmentsAsync(Arg.Any<CancellationToken>())
            .Returns([apt1, apt2]);
        _visitRepo.GetTodayVisitsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit>());

        var query = new GetReceptionistDashboardQuery(null, "Nguyen", 1, 50);

        // Act
        var result = await GetReceptionistDashboardHandler.Handle(
            query, _appointmentRepo, _visitRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].PatientName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public async Task Handle_IncludesWalkInVisits()
    {
        // Arrange
        var walkInVisit = CreateVisit(Guid.NewGuid(), "Walk-In Patient", WorkflowStage.Reception,
            source: VisitSource.WalkIn);

        _appointmentRepo.GetTodayAppointmentsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());
        _visitRepo.GetTodayVisitsAsync(Arg.Any<CancellationToken>())
            .Returns([walkInVisit]);

        var query = new GetReceptionistDashboardQuery(null, null, 1, 50);

        // Act
        var result = await GetReceptionistDashboardHandler.Handle(
            query, _appointmentRepo, _visitRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Source.Should().Be("walkin");
        result.Value.Items[0].PatientName.Should().Be("Walk-In Patient");
    }
}

public class GetReceptionistKpiStatsTests
{
    private readonly IAppointmentRepository _appointmentRepo = Substitute.For<IAppointmentRepository>();
    private readonly IVisitRepository _visitRepo = Substitute.For<IVisitRepository>();

    private static readonly BranchId DefaultBranch = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_ReturnsCorrectCounts()
    {
        // Arrange: 3 appointments, 1 not arrived, 1 waiting, 1 examining
        var apt1 = Appointment.Create(Guid.NewGuid(), "P1", Guid.NewGuid(), "Dr. A",
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(1.5), Guid.NewGuid(), DefaultBranch);
        var apt2 = Appointment.Create(Guid.NewGuid(), "P2", Guid.NewGuid(), "Dr. A",
            DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(2.5), Guid.NewGuid(), DefaultBranch);
        var apt3 = Appointment.Create(Guid.NewGuid(), "P3", Guid.NewGuid(), "Dr. A",
            DateTime.UtcNow.AddHours(3), DateTime.UtcNow.AddHours(3.5), Guid.NewGuid(), DefaultBranch);

        var visit2 = Visit.Create(apt2.PatientId!.Value, "P2", Guid.NewGuid(), "Dr. A",
            DefaultBranch, false, apt2.Id);
        var visit3 = Visit.Create(apt3.PatientId!.Value, "P3", Guid.NewGuid(), "Dr. A",
            DefaultBranch, false, apt3.Id);
        visit3.AdvanceStage(WorkflowStage.PreExam);

        _appointmentRepo.GetTodayAppointmentsAsync(Arg.Any<CancellationToken>())
            .Returns([apt1, apt2, apt3]);
        _visitRepo.GetTodayVisitsAsync(Arg.Any<CancellationToken>())
            .Returns([visit2, visit3]);

        var query = new GetReceptionistKpiStatsQuery();

        // Act
        var result = await GetReceptionistKpiStatsHandler.Handle(
            query, _appointmentRepo, _visitRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TodayAppointments.Should().Be(3);
        result.Value.NotArrived.Should().Be(1);
        result.Value.Waiting.Should().Be(1);
        result.Value.Examining.Should().Be(1);
        result.Value.Completed.Should().Be(0);
    }
}
