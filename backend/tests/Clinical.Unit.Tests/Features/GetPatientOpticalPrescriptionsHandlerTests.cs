using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using FluentAssertions;
using NSubstitute;
using Optical.Contracts.Queries;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// Unit tests for GetPatientOpticalPrescriptionsHandler.
/// Verifies cross-module query returns prescription history from Clinical module.
/// </summary>
public class GetPatientOpticalPrescriptionsHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();

    [Fact]
    public async Task Handle_PatientWithTwoPrescriptions_ReturnsTwoItemsOrderedByDateDescending()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetPatientOpticalPrescriptionsQuery(patientId);
        var visitId1 = Guid.NewGuid();
        var visitId2 = Guid.NewGuid();
        var olderDate = new DateTime(2025, 1, 10, 9, 0, 0, DateTimeKind.Utc);
        var newerDate = new DateTime(2025, 6, 20, 14, 0, 0, DateTimeKind.Utc);

        // Repository returns already-ordered list (newest first)
        var dtos = new List<OpticalPrescriptionHistoryDto>
        {
            new(Guid.NewGuid(), visitId2, newerDate,
                SphOd: -2.0m, CylOd: -0.5m, AxisOd: 90m, AddOd: null,
                SphOs: -1.5m, CylOs: -0.25m, AxisOs: 85m, AddOs: null,
                Pd: 63.0m, Notes: "Second"),
            new(Guid.NewGuid(), visitId1, olderDate,
                SphOd: -1.0m, CylOd: null, AxisOd: null, AddOd: null,
                SphOs: -1.25m, CylOs: null, AxisOs: null, AddOs: null,
                Pd: 62.0m, Notes: "First")
        };

        _visitRepository.GetOpticalPrescriptionsByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(dtos);

        // Act
        var result = await GetPatientOpticalPrescriptionsHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Notes.Should().Be("Second");   // Most recent first
        result[1].Notes.Should().Be("First");
        result[0].VisitDate.Should().Be(newerDate);
        result[1].VisitDate.Should().Be(olderDate);
    }

    [Fact]
    public async Task Handle_PatientWithNoPrescriptions_ReturnsEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetPatientOpticalPrescriptionsQuery(patientId);

        _visitRepository.GetOpticalPrescriptionsByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto>());

        // Act
        var result = await GetPatientOpticalPrescriptionsHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FieldMappingIsCorrect_AllFieldsPassedThrough()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetPatientOpticalPrescriptionsQuery(patientId);
        var rxId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var visitDate = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        var dtos = new List<OpticalPrescriptionHistoryDto>
        {
            new(rxId, visitId, visitDate,
                SphOd: -2.50m, CylOd: -0.75m, AxisOd: 90m, AddOd: 1.50m,
                SphOs: -3.00m, CylOs: -1.00m, AxisOs: 85m, AddOs: 1.50m,
                Pd: 63.5m, Notes: "Test notes")
        };

        _visitRepository.GetOpticalPrescriptionsByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(dtos);

        // Act
        var result = await GetPatientOpticalPrescriptionsHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be(rxId);
        dto.VisitId.Should().Be(visitId);
        dto.VisitDate.Should().Be(visitDate);
        dto.SphOd.Should().Be(-2.50m);
        dto.CylOd.Should().Be(-0.75m);
        dto.AxisOd.Should().Be(90m);
        dto.AddOd.Should().Be(1.50m);
        dto.SphOs.Should().Be(-3.00m);
        dto.CylOs.Should().Be(-1.00m);
        dto.AxisOs.Should().Be(85m);
        dto.AddOs.Should().Be(1.50m);
        dto.Pd.Should().Be(63.5m);
        dto.Notes.Should().Be("Test notes");
    }
}
