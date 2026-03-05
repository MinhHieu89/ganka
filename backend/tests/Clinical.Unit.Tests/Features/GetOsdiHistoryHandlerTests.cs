using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class GetOsdiHistoryHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_PatientWith3Visits_Returns3DataPoints()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        // Create 3 assessments with OSDI scores
        var assessments = new List<DryEyeAssessment>();
        for (int i = 0; i < 3; i++)
        {
            var assessment = DryEyeAssessment.Create(Guid.NewGuid());
            assessment.SetOsdiScore(20m + i * 10, i == 0 ? OsdiSeverity.Mild : i == 1 ? OsdiSeverity.Moderate : OsdiSeverity.Severe);
            assessments.Add(assessment);
        }

        _visitRepository.GetDryEyeAssessmentsByPatientAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(assessments);

        // We need visit dates -- the handler needs to get them. Let's set up visits for the assessments.
        foreach (var assessment in assessments)
        {
            var visit = Visit.Create(patientId, "Patient", Guid.NewGuid(), "Dr.", DefaultBranchId, false);
            typeof(Entity).GetProperty("Id")!.GetSetMethod(true)!.Invoke(visit, [assessment.VisitId]);
            _visitRepository.GetByIdAsync(assessment.VisitId, Arg.Any<CancellationToken>()).Returns(visit);
        }

        var query = new GetOsdiHistoryQuery(patientId);

        // Act
        var result = await GetOsdiHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Should().AllSatisfy(item =>
        {
            item.OsdiScore.Should().BeGreaterThan(0);
            item.VisitId.Should().NotBeEmpty();
        });
    }

    [Fact]
    public async Task Handle_PatientWithNoVisits_ReturnsEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetDryEyeAssessmentsByPatientAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<DryEyeAssessment>());

        var query = new GetOsdiHistoryQuery(patientId);

        // Act
        var result = await GetOsdiHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_VisitsWithNoDryEye_ReturnsEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetDryEyeAssessmentsByPatientAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<DryEyeAssessment>());

        var query = new GetOsdiHistoryQuery(patientId);

        // Act
        var result = await GetOsdiHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_OnlyReturnsAssessmentsWithOsdiScore()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        // One with score, one without
        var withScore = DryEyeAssessment.Create(Guid.NewGuid());
        withScore.SetOsdiScore(45m, OsdiSeverity.Severe);

        var withoutScore = DryEyeAssessment.Create(Guid.NewGuid());
        // No OSDI score set -> OsdiScore is null

        _visitRepository.GetDryEyeAssessmentsByPatientAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<DryEyeAssessment> { withScore, withoutScore });

        // Set up visit for the one with score
        var visit = Visit.Create(patientId, "Patient", Guid.NewGuid(), "Dr.", DefaultBranchId, false);
        typeof(Entity).GetProperty("Id")!.GetSetMethod(true)!.Invoke(visit, [withScore.VisitId]);
        _visitRepository.GetByIdAsync(withScore.VisitId, Arg.Any<CancellationToken>()).Returns(visit);

        var query = new GetOsdiHistoryQuery(patientId);

        // Act
        var result = await GetOsdiHistoryHandler.Handle(query, _visitRepository, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].OsdiScore.Should().Be(45m);
    }
}
