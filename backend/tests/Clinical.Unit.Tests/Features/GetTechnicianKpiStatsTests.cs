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
/// TDD tests for the technician KPI stats handler.
/// Validates correct count derivation per D-09.
/// </summary>
public class GetTechnicianKpiStatsTests : IDisposable
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    private readonly ClinicalDbContext _dbContext;
    private readonly Guid _technicianId = Guid.NewGuid();
    private readonly Guid _otherTechnicianId = Guid.NewGuid();

    public GetTechnicianKpiStatsTests()
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

    private Visit CreateVisitAtPreExam(string patientName = "Patient")
    {
        var visit = Visit.Create(Guid.NewGuid(), patientName, Guid.NewGuid(), "Dr. B",
            DefaultBranchId, false);
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
    public async Task Handle_ReturnsCorrectCounts()
    {
        // Arrange: 2 waiting, 1 in-progress (mine), 1 completed, 1 red flag
        var v1 = CreateVisitAtPreExam("Waiting 1");
        await SeedVisitWithOrder(v1);

        var v2 = CreateVisitAtPreExam("Waiting 2");
        await SeedVisitWithOrder(v2);

        var v3 = CreateVisitAtPreExam("In Progress");
        await SeedVisitWithOrder(v3, o => o.Accept(_technicianId, "Tech A"));

        var v4 = CreateVisitAtPreExam("Completed");
        await SeedVisitWithOrder(v4, o =>
        {
            o.Accept(_technicianId, "Tech A");
            o.Complete();
        });

        var v5 = CreateVisitAtPreExam("Red Flag");
        await SeedVisitWithOrder(v5, o =>
        {
            o.Accept(_technicianId, "Tech A");
            o.MarkRedFlag("Cannot test");
        });

        var query = new GetTechnicianKpiQuery(_technicianId);

        // Act
        var result = await GetTechnicianKpiStatsHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Waiting.Should().Be(2);
        result.InProgress.Should().Be(1);
        result.Completed.Should().Be(1);
        result.RedFlag.Should().Be(1);
    }

    [Fact]
    public async Task Handle_InProgressCountsOnlyCurrentTechnician()
    {
        // Arrange: 1 accepted by current tech, 1 accepted by another tech
        var v1 = CreateVisitAtPreExam("My Patient");
        await SeedVisitWithOrder(v1, o => o.Accept(_technicianId, "Tech A"));

        var v2 = CreateVisitAtPreExam("Other Patient");
        await SeedVisitWithOrder(v2, o => o.Accept(_otherTechnicianId, "Tech B"));

        var query = new GetTechnicianKpiQuery(_technicianId);

        // Act
        var result = await GetTechnicianKpiStatsHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.InProgress.Should().Be(1);
    }

    [Fact]
    public async Task Handle_OnlyCountsTodayOrders()
    {
        // Arrange: create a visit with order that has OrderedAt set to yesterday
        var visit = CreateVisitAtPreExam("Yesterday Patient");
        _dbContext.Visits.Add(visit);
        var order = visit.TechnicianOrders.First();
        // Set OrderedAt to yesterday via reflection
        typeof(TechnicianOrder).GetProperty("OrderedAt")!.SetValue(order, DateTime.UtcNow.AddDays(-1));
        await _dbContext.SaveChangesAsync();

        // Also create a today order
        var todayVisit = CreateVisitAtPreExam("Today Patient");
        await SeedVisitWithOrder(todayVisit);

        var query = new GetTechnicianKpiQuery(_technicianId);

        // Act
        var result = await GetTechnicianKpiStatsHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Waiting.Should().Be(1); // Only today's
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsZeroCounts()
    {
        var query = new GetTechnicianKpiQuery(_technicianId);

        // Act
        var result = await GetTechnicianKpiStatsHandler.Handle(query, _dbContext, CancellationToken.None);

        // Assert
        result.Waiting.Should().Be(0);
        result.InProgress.Should().Be(0);
        result.Completed.Should().Be(0);
        result.RedFlag.Should().Be(0);
    }
}
