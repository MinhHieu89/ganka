using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Clinical.Infrastructure;
using Clinical.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// TDD tests for the technician KPI stats handler.
/// Validates correct count derivation per D-09.
/// Uses InMemory DB with real TechnicianOrderQueryService.
/// </summary>
public class GetTechnicianKpiStatsTests : IDisposable
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    private readonly ClinicalDbContext _dbContext;
    private readonly ITechnicianOrderQueryService _queryService;
    private readonly Guid _technicianId = Guid.NewGuid();
    private readonly Guid _otherTechnicianId = Guid.NewGuid();

    public GetTechnicianKpiStatsTests()
    {
        var options = new DbContextOptionsBuilder<ClinicalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ClinicalDbContext(options);
        _queryService = new TechnicianOrderQueryService(_dbContext);
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
        visit.CreatePreExamOrder();
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
        await SeedVisitWithOrder(CreateVisitAtPreExam("Waiting 1"));
        await SeedVisitWithOrder(CreateVisitAtPreExam("Waiting 2"));
        await SeedVisitWithOrder(CreateVisitAtPreExam("In Progress"),
            o => o.Accept(_technicianId, "Tech A"));
        await SeedVisitWithOrder(CreateVisitAtPreExam("Completed"), o =>
        {
            o.Accept(_technicianId, "Tech A");
            o.Complete();
        });
        await SeedVisitWithOrder(CreateVisitAtPreExam("Red Flag"), o =>
        {
            o.Accept(_technicianId, "Tech A");
            o.MarkRedFlag("Cannot test");
        });

        var result = await GetTechnicianKpiStatsHandler.Handle(
            new GetTechnicianKpiQuery(_technicianId), _queryService, CancellationToken.None);

        result.Waiting.Should().Be(2);
        result.InProgress.Should().Be(1);
        result.Completed.Should().Be(1);
        result.RedFlag.Should().Be(1);
    }

    [Fact]
    public async Task Handle_InProgressCountsOnlyCurrentTechnician()
    {
        await SeedVisitWithOrder(CreateVisitAtPreExam("My Patient"),
            o => o.Accept(_technicianId, "Tech A"));
        await SeedVisitWithOrder(CreateVisitAtPreExam("Other Patient"),
            o => o.Accept(_otherTechnicianId, "Tech B"));

        var result = await GetTechnicianKpiStatsHandler.Handle(
            new GetTechnicianKpiQuery(_technicianId), _queryService, CancellationToken.None);

        result.InProgress.Should().Be(1);
    }

    [Fact]
    public async Task Handle_OnlyCountsTodayOrders()
    {
        // Create a visit with order set to yesterday
        var yesterdayVisit = CreateVisitAtPreExam("Yesterday Patient");
        _dbContext.Visits.Add(yesterdayVisit);
        var order = yesterdayVisit.TechnicianOrders.First();
        typeof(TechnicianOrder).GetProperty("OrderedAt")!.SetValue(order, DateTime.UtcNow.AddDays(-1));
        await _dbContext.SaveChangesAsync();

        // Create a today order
        await SeedVisitWithOrder(CreateVisitAtPreExam("Today Patient"));

        var result = await GetTechnicianKpiStatsHandler.Handle(
            new GetTechnicianKpiQuery(_technicianId), _queryService, CancellationToken.None);

        result.Waiting.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsZeroCounts()
    {
        var result = await GetTechnicianKpiStatsHandler.Handle(
            new GetTechnicianKpiQuery(_technicianId), _queryService, CancellationToken.None);

        result.Waiting.Should().Be(0);
        result.InProgress.Should().Be(0);
        result.Completed.Should().Be(0);
        result.RedFlag.Should().Be(0);
    }
}
