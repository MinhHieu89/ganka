using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// Tests for GetPatientVisitHistoryHandler.
/// </summary>
public class GetPatientVisitHistoryHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_ReturnsVisitsOrderedByDateDescending()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit1 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        var visit2 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. B", DefaultBranchId, false);

        _visitRepository.GetVisitsByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { visit1, visit2 });

        var query = new GetPatientVisitHistoryQuery(patientId);

        // Act
        var result = await GetPatientVisitHistoryHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].VisitDate.Should().BeOnOrAfter(result[1].VisitDate);
    }

    [Fact]
    public async Task Handle_PatientWithNoVisits_ReturnsEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetVisitsByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<Visit>());

        var query = new GetPatientVisitHistoryQuery(patientId);

        // Act
        var result = await GetPatientVisitHistoryHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsPrimaryDiagnosisText()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        // Add a primary diagnosis
        var diagnosis = VisitDiagnosis.Create(
            visit.Id, "H04.1", "Dry Eye", "Kho mat",
            Laterality.OU, DiagnosisRole.Primary, 0);
        visit.AddDiagnosis(diagnosis);

        _visitRepository.GetVisitsByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { visit });

        var query = new GetPatientVisitHistoryQuery(patientId);

        // Act
        var result = await GetPatientVisitHistoryHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].PrimaryDiagnosisText.Should().Be("Kho mat");
    }
}
