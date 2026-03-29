using Clinical.Application.Features;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Clinical.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// TDD tests for the technician dashboard query handler.
/// Tests status derivation per D-08 and visit type detection per D-10.
/// </summary>
public class GetTechnicianDashboardTests : IDisposable
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    private readonly ClinicalDbContext _dbContext;
    private readonly Guid _technicianId = Guid.NewGuid();

    public GetTechnicianDashboardTests()
    {
        var options = new DbContextOptionsBuilder<ClinicalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ClinicalDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private Visit CreateVisitAtPreExam(Guid? patientId = null, string patientName = "Nguyen Van A",
        DateTime? visitDate = null, string? reason = null)
    {
        var pid = patientId ?? Guid.NewGuid();
        var visit = Visit.Create(pid, patientName, Guid.NewGuid(), "Dr. B", DefaultBranchId, false,
            reason: reason);

        // Override VisitDate if needed via reflection (factory sets it to UtcNow)
        if (visitDate.HasValue)
        {
            typeof(Visit).GetProperty("VisitDate")!.SetValue(visit, visitDate.Value);
        }

        visit.AdvanceStage(WorkflowStage.PreExam);
        return visit;
    }

    private async Task SeedVisitWithOrder(Visit visit, Action<TechnicianOrder>? configureOrder = null)
    {
        _dbContext.Visits.Add(visit);
        var order = visit.TechnicianOrders.First();
        configureOrder?.Invoke(order);
        await _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ReturnsWaitingStatus_WhenOrderNotAccepted()
    {
        // Arrange
        var visit = CreateVisitAtPreExam();
        await SeedVisitWithOrder(visit);

        var query = new GetTechnicianDashboardQuery(null, null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be("waiting");
    }

    [Fact]
    public async Task Handle_ReturnsInProgressStatus_WhenOrderAcceptedByCurrentTechnician()
    {
        // Arrange
        var visit = CreateVisitAtPreExam();
        await SeedVisitWithOrder(visit, order => order.Accept(_technicianId, "Tech A"));

        var query = new GetTechnicianDashboardQuery(null, null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle(r => r.Status == "in_progress");
    }

    [Fact]
    public async Task Handle_ReturnsRedFlagStatus_WhenOrderIsRedFlagged()
    {
        // Arrange
        var visit = CreateVisitAtPreExam();
        await SeedVisitWithOrder(visit, order =>
        {
            order.Accept(_technicianId, "Tech A");
            order.MarkRedFlag("Patient uncooperative");
        });

        var query = new GetTechnicianDashboardQuery(null, null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle(r => r.Status == "red_flag");
    }

    [Fact]
    public async Task Handle_ReturnsCompletedStatus_WhenOrderIsCompleted()
    {
        // Arrange
        var visit = CreateVisitAtPreExam();
        await SeedVisitWithOrder(visit, order =>
        {
            order.Accept(_technicianId, "Tech A");
            order.Complete();
        });

        var query = new GetTechnicianDashboardQuery(null, null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle(r => r.Status == "completed");
    }

    [Fact]
    public async Task Handle_FiltersByStatus_ReturnsOnlyMatchingRows()
    {
        // Arrange: 1 waiting, 1 completed
        var visit1 = CreateVisitAtPreExam(patientName: "Patient 1");
        await SeedVisitWithOrder(visit1);
        var visit2 = CreateVisitAtPreExam(patientName: "Patient 2");
        await SeedVisitWithOrder(visit2, order =>
        {
            order.Accept(_technicianId, "Tech A");
            order.Complete();
        });

        var query = new GetTechnicianDashboardQuery("waiting", null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be("waiting");
    }

    [Fact]
    public async Task Handle_SearchByPatientName_CaseInsensitive()
    {
        // Arrange
        var visit1 = CreateVisitAtPreExam(patientName: "Nguyen Van A");
        await SeedVisitWithOrder(visit1);
        var visit2 = CreateVisitAtPreExam(patientName: "Tran Thi B");
        await SeedVisitWithOrder(visit2);

        var query = new GetTechnicianDashboardQuery(null, "nguyen", CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].PatientName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public async Task Handle_RowIncludesExpectedFields()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit = CreateVisitAtPreExam(patientId: patientId, patientName: "Patient X", reason: "Eye check");
        await SeedVisitWithOrder(visit);

        var query = new GetTechnicianDashboardQuery(null, null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        var row = result.Items[0];
        row.PatientId.Should().Be(patientId);
        row.PatientName.Should().Be("Patient X");
        row.VisitId.Should().Be(visit.Id);
        row.Reason.Should().Be("Eye check");
        row.OrderId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_VisitType_NewWhenNoOtherVisits()
    {
        // Arrange: single visit for patient
        var patientId = Guid.NewGuid();
        var visit = CreateVisitAtPreExam(patientId: patientId);
        await SeedVisitWithOrder(visit);

        var query = new GetTechnicianDashboardQuery(null, null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Items[0].VisitType.Should().Be("new");
    }

    [Fact]
    public async Task Handle_VisitType_FollowUpWhenPatientHasPriorVisits()
    {
        // Arrange: patient has a prior visit
        var patientId = Guid.NewGuid();

        // Create a prior visit (completed)
        var priorVisit = Visit.Create(patientId, "Returning Patient", Guid.NewGuid(), "Dr. B",
            DefaultBranchId, false);
        typeof(Visit).GetProperty("VisitDate")!.SetValue(priorVisit,
            DateTime.UtcNow.AddDays(-30));
        _dbContext.Visits.Add(priorVisit);
        await _dbContext.SaveChangesAsync();

        // Create today's visit
        var visit = CreateVisitAtPreExam(patientId: patientId, patientName: "Returning Patient");
        await SeedVisitWithOrder(visit);

        var query = new GetTechnicianDashboardQuery(null, null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle(r => r.VisitType == "follow_up");
    }

    [Fact]
    public async Task Handle_SortsInProgressFirst_ThenByCheckinTime()
    {
        // Arrange: create waiting and in-progress orders
        var visit1 = CreateVisitAtPreExam(patientName: "Early Patient");
        await SeedVisitWithOrder(visit1);

        var visit2 = CreateVisitAtPreExam(patientName: "In Progress Patient");
        await SeedVisitWithOrder(visit2, order => order.Accept(_technicianId, "Tech A"));

        var query = new GetTechnicianDashboardQuery(null, null, CurrentTechnicianId: _technicianId);

        // Act
        var result = await GetTechnicianDashboardHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert - in_progress should come first
        result.Items[0].Status.Should().Be("in_progress");
    }
}
