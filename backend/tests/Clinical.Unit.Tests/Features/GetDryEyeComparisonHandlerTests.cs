using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class GetDryEyeComparisonHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_TwoValidVisitsSamePatient_ReturnsBothAssessments()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit1 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        var visit2 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);

        // Add dry eye assessment to visit1
        var assessment1 = DryEyeAssessment.Create(visit1.Id);
        assessment1.Update(10m, 9m, 15m, 12m, 1, 2, 0.3m, 0.25m, 2, 3);
        visit1.AddDryEyeAssessment(assessment1);

        // Add dry eye assessment to visit2
        var assessment2 = DryEyeAssessment.Create(visit2.Id);
        assessment2.Update(12m, 11m, 18m, 15m, 0, 1, 0.4m, 0.35m, 1, 2);
        visit2.AddDryEyeAssessment(assessment2);

        _visitRepository.GetByIdWithDetailsAsync(visit1.Id, Arg.Any<CancellationToken>()).Returns(visit1);
        _visitRepository.GetByIdWithDetailsAsync(visit2.Id, Arg.Any<CancellationToken>()).Returns(visit2);

        var query = new GetDryEyeComparisonQuery(patientId, visit1.Id, visit2.Id);

        // Act
        var result = await GetDryEyeComparisonHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Visit1.Assessment.Should().NotBeNull();
        result.Value.Visit2.Assessment.Should().NotBeNull();
        result.Value.Visit1.Assessment!.OdTbut.Should().Be(10m);
        result.Value.Visit2.Assessment!.OdTbut.Should().Be(12m);
    }

    [Fact]
    public async Task Handle_Visit1HasData_Visit2HasNone_ReturnsNullForVisit2()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit1 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        var visit2 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);

        // Only visit1 has assessment
        var assessment1 = DryEyeAssessment.Create(visit1.Id);
        assessment1.Update(10m, 9m, 15m, 12m, 1, 2, 0.3m, 0.25m, 2, 3);
        visit1.AddDryEyeAssessment(assessment1);

        _visitRepository.GetByIdWithDetailsAsync(visit1.Id, Arg.Any<CancellationToken>()).Returns(visit1);
        _visitRepository.GetByIdWithDetailsAsync(visit2.Id, Arg.Any<CancellationToken>()).Returns(visit2);

        var query = new GetDryEyeComparisonQuery(patientId, visit1.Id, visit2.Id);

        // Act
        var result = await GetDryEyeComparisonHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Visit1.Assessment.Should().NotBeNull();
        result.Value.Visit2.Assessment.Should().BeNull();
    }

    [Fact]
    public async Task Handle_BothVisitsNoData_ReturnsNullForBoth()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit1 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        var visit2 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);

        _visitRepository.GetByIdWithDetailsAsync(visit1.Id, Arg.Any<CancellationToken>()).Returns(visit1);
        _visitRepository.GetByIdWithDetailsAsync(visit2.Id, Arg.Any<CancellationToken>()).Returns(visit2);

        var query = new GetDryEyeComparisonQuery(patientId, visit1.Id, visit2.Id);

        // Act
        var result = await GetDryEyeComparisonHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Visit1.Assessment.Should().BeNull();
        result.Value.Visit2.Assessment.Should().BeNull();
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsFailure()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Visit?)null);

        var query = new GetDryEyeComparisonQuery(patientId, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await GetDryEyeComparisonHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DifferentPatients_ReturnsFailure()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var otherPatientId = Guid.NewGuid();

        var visit1 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        var visit2 = Visit.Create(otherPatientId, "Patient B", Guid.NewGuid(), "Dr. B", DefaultBranchId, false);

        _visitRepository.GetByIdWithDetailsAsync(visit1.Id, Arg.Any<CancellationToken>()).Returns(visit1);
        _visitRepository.GetByIdWithDetailsAsync(visit2.Id, Arg.Any<CancellationToken>()).Returns(visit2);

        var query = new GetDryEyeComparisonQuery(patientId, visit1.Id, visit2.Id);

        // Act
        var result = await GetDryEyeComparisonHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
