using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class GetDryEyeMetricHistoryHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_PatientWithAssessments_Returns5MetricTimeSeries()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var visitDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        var assessment = DryEyeAssessment.Create(visitId);
        assessment.Update(10m, 9m, 15m, 12m, 1, 2, 0.3m, 0.25m, 2, 3);

        _visitRepository.GetMetricHistoryAsync(patientId, null, Arg.Any<CancellationToken>())
            .Returns(new List<(DryEyeAssessment Assessment, DateTime VisitDate)>
            {
                (assessment, visitDate)
            });

        var query = new GetDryEyeMetricHistoryQuery(patientId, "all");

        // Act
        var result = await GetDryEyeMetricHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        result.Metrics.Should().HaveCount(5);
        result.Metrics.Select(m => m.MetricName).Should()
            .BeEquivalentTo(["TBUT", "Schirmer", "MeibomianGrading", "TearMeniscus", "StainingScore"]);
    }

    [Fact]
    public async Task Handle_PatientWithAssessments_ReturnsCorrectOdOsValues()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var visitDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        var assessment = DryEyeAssessment.Create(visitId);
        assessment.Update(10m, 9m, 15m, 12m, 1, 2, 0.3m, 0.25m, 2, 3);

        _visitRepository.GetMetricHistoryAsync(patientId, null, Arg.Any<CancellationToken>())
            .Returns(new List<(DryEyeAssessment Assessment, DateTime VisitDate)>
            {
                (assessment, visitDate)
            });

        var query = new GetDryEyeMetricHistoryQuery(patientId, "all");

        // Act
        var result = await GetDryEyeMetricHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        var tbut = result.Metrics.First(m => m.MetricName == "TBUT");
        tbut.DataPoints.Should().HaveCount(1);
        tbut.DataPoints[0].OdValue.Should().Be(10m);
        tbut.DataPoints[0].OsValue.Should().Be(9m);
        tbut.DataPoints[0].VisitDate.Should().Be(visitDate);

        var schirmer = result.Metrics.First(m => m.MetricName == "Schirmer");
        schirmer.DataPoints[0].OdValue.Should().Be(15m);
        schirmer.DataPoints[0].OsValue.Should().Be(12m);

        var meibomian = result.Metrics.First(m => m.MetricName == "MeibomianGrading");
        meibomian.DataPoints[0].OdValue.Should().Be(1m);
        meibomian.DataPoints[0].OsValue.Should().Be(2m);

        var tearMeniscus = result.Metrics.First(m => m.MetricName == "TearMeniscus");
        tearMeniscus.DataPoints[0].OdValue.Should().Be(0.3m);
        tearMeniscus.DataPoints[0].OsValue.Should().Be(0.25m);

        var staining = result.Metrics.First(m => m.MetricName == "StainingScore");
        staining.DataPoints[0].OdValue.Should().Be(2m);
        staining.DataPoints[0].OsValue.Should().Be(3m);
    }

    [Fact]
    public async Task Handle_NoAssessments_ReturnsEmptyMetrics()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetMetricHistoryAsync(patientId, null, Arg.Any<CancellationToken>())
            .Returns(new List<(DryEyeAssessment Assessment, DateTime VisitDate)>());

        var query = new GetDryEyeMetricHistoryQuery(patientId, "all");

        // Act
        var result = await GetDryEyeMetricHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        result.Metrics.Should().HaveCount(5);
        result.Metrics.Should().AllSatisfy(m => m.DataPoints.Should().BeEmpty());
    }

    [Fact]
    public async Task Handle_TimeRange3m_PassesCorrectCutoffDate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetMetricHistoryAsync(patientId, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(new List<(DryEyeAssessment Assessment, DateTime VisitDate)>());

        var query = new GetDryEyeMetricHistoryQuery(patientId, "3m");

        // Act
        await GetDryEyeMetricHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert - verify that a non-null cutoff date was passed (approximately 3 months ago)
        await _visitRepository.Received(1).GetMetricHistoryAsync(
            patientId,
            Arg.Is<DateTime?>(d => d.HasValue && d.Value > DateTime.UtcNow.AddMonths(-3).AddMinutes(-1)
                                   && d.Value < DateTime.UtcNow.AddMonths(-3).AddMinutes(1)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TimeRange6m_PassesCorrectCutoffDate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetMetricHistoryAsync(patientId, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(new List<(DryEyeAssessment Assessment, DateTime VisitDate)>());

        var query = new GetDryEyeMetricHistoryQuery(patientId, "6m");

        // Act
        await GetDryEyeMetricHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        await _visitRepository.Received(1).GetMetricHistoryAsync(
            patientId,
            Arg.Is<DateTime?>(d => d.HasValue && d.Value > DateTime.UtcNow.AddMonths(-6).AddMinutes(-1)
                                   && d.Value < DateTime.UtcNow.AddMonths(-6).AddMinutes(1)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TimeRange1y_PassesCorrectCutoffDate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetMetricHistoryAsync(patientId, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(new List<(DryEyeAssessment Assessment, DateTime VisitDate)>());

        var query = new GetDryEyeMetricHistoryQuery(patientId, "1y");

        // Act
        await GetDryEyeMetricHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        await _visitRepository.Received(1).GetMetricHistoryAsync(
            patientId,
            Arg.Is<DateTime?>(d => d.HasValue && d.Value > DateTime.UtcNow.AddYears(-1).AddMinutes(-1)
                                   && d.Value < DateTime.UtcNow.AddYears(-1).AddMinutes(1)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TimeRangeAll_PassesNullCutoff()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetMetricHistoryAsync(patientId, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(new List<(DryEyeAssessment Assessment, DateTime VisitDate)>());

        var query = new GetDryEyeMetricHistoryQuery(patientId, "all");

        // Act
        await GetDryEyeMetricHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        await _visitRepository.Received(1).GetMetricHistoryAsync(
            patientId,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleAssessments_OrderedByVisitDateAscending()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var date1 = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);

        var a1 = DryEyeAssessment.Create(Guid.NewGuid());
        a1.Update(5m, 4m, 10m, 8m, 0, 1, 0.2m, 0.15m, 1, 2);

        var a2 = DryEyeAssessment.Create(Guid.NewGuid());
        a2.Update(8m, 7m, 12m, 10m, 1, 1, 0.25m, 0.2m, 1, 1);

        var a3 = DryEyeAssessment.Create(Guid.NewGuid());
        a3.Update(10m, 9m, 15m, 13m, 0, 0, 0.3m, 0.25m, 0, 1);

        // Return in order (repository should order by visit date ASC)
        _visitRepository.GetMetricHistoryAsync(patientId, null, Arg.Any<CancellationToken>())
            .Returns(new List<(DryEyeAssessment Assessment, DateTime VisitDate)>
            {
                (a1, date1), (a2, date2), (a3, date3)
            });

        var query = new GetDryEyeMetricHistoryQuery(patientId, "all");

        // Act
        var result = await GetDryEyeMetricHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        var tbut = result.Metrics.First(m => m.MetricName == "TBUT");
        tbut.DataPoints.Should().HaveCount(3);
        tbut.DataPoints[0].VisitDate.Should().Be(date1);
        tbut.DataPoints[1].VisitDate.Should().Be(date2);
        tbut.DataPoints[2].VisitDate.Should().Be(date3);
        tbut.DataPoints[0].OdValue.Should().Be(5m);
        tbut.DataPoints[1].OdValue.Should().Be(8m);
        tbut.DataPoints[2].OdValue.Should().Be(10m);
    }
}
