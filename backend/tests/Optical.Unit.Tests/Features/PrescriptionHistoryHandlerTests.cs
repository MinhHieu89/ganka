using FluentAssertions;
using NSubstitute;
using Optical.Application.Features.Prescriptions;
using Optical.Contracts.Queries;
using Wolverine;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// TDD tests for prescription history and comparison handlers:
///   - GetPatientPrescriptionHistoryHandler (OPT-08)
///   - GetPrescriptionComparisonHandler (OPT-08)
/// Uses IMessageBus mock to simulate cross-module query to Clinical module.
/// </summary>
public class PrescriptionHistoryHandlerTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    // ─── Sample Data Helpers ─────────────────────────────────────────────────

    private static OpticalPrescriptionHistoryDto CreateDto(
        Guid? id = null,
        Guid? visitId = null,
        DateTime? visitDate = null,
        decimal? sphOd = -2.00m,
        decimal? cylOd = -0.50m,
        decimal? axisOd = 90m,
        decimal? addOd = null,
        decimal? sphOs = -1.50m,
        decimal? cylOs = -0.25m,
        decimal? axisOs = 85m,
        decimal? addOs = null,
        decimal? pd = 63.0m,
        string? notes = null)
        => new OpticalPrescriptionHistoryDto(
            Id: id ?? Guid.NewGuid(),
            VisitId: visitId ?? Guid.NewGuid(),
            VisitDate: visitDate ?? DateTime.UtcNow,
            SphOd: sphOd,
            CylOd: cylOd,
            AxisOd: axisOd,
            AddOd: addOd,
            SphOs: sphOs,
            CylOs: cylOs,
            AxisOs: axisOs,
            AddOs: addOs,
            Pd: pd,
            Notes: notes);

    // ─── GetPatientPrescriptionHistory Tests ─────────────────────────────────

    [Fact]
    public async Task GetPrescriptionHistory_PatientWithPrescriptions_ReturnsSortedByDateDesc()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var oldDto = CreateDto(visitDate: new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc), notes: "Older");
        var newDto = CreateDto(visitDate: new DateTime(2025, 6, 20, 0, 0, 0, DateTimeKind.Utc), notes: "Newer");

        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Is<GetPatientOpticalPrescriptionsQuery>(q => q.PatientId == patientId),
                Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto> { oldDto, newDto });

        var query = new GetPatientPrescriptionHistoryQuery(patientId);

        // Act
        var result = await GetPatientPrescriptionHistoryHandler.Handle(
            query, _bus, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Notes.Should().Be("Newer"); // Most recent first
        result[1].Notes.Should().Be("Older");
    }

    [Fact]
    public async Task GetPrescriptionHistory_NullFromBus_ReturnsEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Any<GetPatientOpticalPrescriptionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns((List<OpticalPrescriptionHistoryDto>?)null);

        var query = new GetPatientPrescriptionHistoryQuery(patientId);

        // Act
        var result = await GetPatientPrescriptionHistoryHandler.Handle(
            query, _bus, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionHistory_EmptyListFromBus_ReturnsEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Any<GetPatientOpticalPrescriptionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto>());

        var query = new GetPatientPrescriptionHistoryQuery(patientId);

        // Act
        var result = await GetPatientPrescriptionHistoryHandler.Handle(
            query, _bus, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionHistory_SendsCorrectQueryToBus()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Any<GetPatientOpticalPrescriptionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto>());

        var query = new GetPatientPrescriptionHistoryQuery(patientId);

        // Act
        await GetPatientPrescriptionHistoryHandler.Handle(query, _bus, CancellationToken.None);

        // Assert - verify it passed the correct patientId to Clinical module
        await _bus.Received(1).InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
            Arg.Is<GetPatientOpticalPrescriptionsQuery>(q => q.PatientId == patientId),
            Arg.Any<CancellationToken>());
    }

    // ─── GetPrescriptionComparison Tests ─────────────────────────────────────

    [Fact]
    public async Task GetPrescriptionComparison_TwoPrescriptions_ReturnsComparisonWithOlderAndNewer()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var older = CreateDto(id: id1, visitDate: new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc), sphOd: -1.00m, notes: "Older Rx");
        var newer = CreateDto(id: id2, visitDate: new DateTime(2025, 7, 5, 0, 0, 0, DateTimeKind.Utc), sphOd: -2.00m, notes: "Newer Rx");

        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Any<GetPatientOpticalPrescriptionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto> { older, newer });

        var query = new GetPrescriptionComparisonQuery(patientId, id1, id2);

        // Act
        var result = await GetPrescriptionComparisonHandler.Handle(
            query, _bus, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Older.Id.Should().Be(id1);
        result.Newer.Id.Should().Be(id2);
        result.Older.VisitDate.Should().BeBefore(result.Newer.VisitDate);
    }

    [Fact]
    public async Task GetPrescriptionComparison_IdentifiesChangedFields()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var older = CreateDto(id: id1, visitDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            sphOd: -1.00m, cylOd: -0.25m, sphOs: -0.75m, cylOs: -0.50m);
        var newer = CreateDto(id: id2, visitDate: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            sphOd: -1.50m, cylOd: -0.25m, sphOs: -0.75m, cylOs: -0.25m);

        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Any<GetPatientOpticalPrescriptionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto> { older, newer });

        var query = new GetPrescriptionComparisonQuery(patientId, id1, id2);

        // Act
        var result = await GetPrescriptionComparisonHandler.Handle(
            query, _bus, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Changes.Should().NotBeNull();
        // SphOd changed: -1.00 -> -1.50 (worsened for myopia)
        var sphOdChange = result.Changes.FirstOrDefault(c => c.FieldName == "SphOd");
        sphOdChange.Should().NotBeNull();
        sphOdChange!.OldValue.Should().Be("-1.00");
        sphOdChange.NewValue.Should().Be("-1.50");
        // CylOs improved: -0.50 -> -0.25
        var cylOsChange = result.Changes.FirstOrDefault(c => c.FieldName == "CylOs");
        cylOsChange.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrescriptionComparison_NoChanges_ReturnsEmptyChangesList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Both prescriptions are identical (except date and id)
        var older = CreateDto(id: id1, visitDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            sphOd: -2.00m, cylOd: -0.50m, sphOs: -1.50m, cylOs: -0.25m, pd: 63.0m);
        var newer = CreateDto(id: id2, visitDate: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            sphOd: -2.00m, cylOd: -0.50m, sphOs: -1.50m, cylOs: -0.25m, pd: 63.0m);

        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Any<GetPatientOpticalPrescriptionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto> { older, newer });

        var query = new GetPrescriptionComparisonQuery(patientId, id1, id2);

        // Act
        var result = await GetPrescriptionComparisonHandler.Handle(
            query, _bus, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionComparison_PrescriptionNotFound_ReturnsNull()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Only one prescription returned, so second won't be found
        var dto = CreateDto(id: id1, visitDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Any<GetPatientOpticalPrescriptionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto> { dto });

        var query = new GetPrescriptionComparisonQuery(patientId, id1, id2);

        // Act
        var result = await GetPrescriptionComparisonHandler.Handle(
            query, _bus, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPrescriptionComparison_CorrectlyDeterminesOlderVsNewer_ByDate()
    {
        // Arrange - provide prescription IDs in reverse order (newer first)
        var patientId = Guid.NewGuid();
        var olderId = Guid.NewGuid();
        var newerId = Guid.NewGuid();

        var older = CreateDto(id: olderId, visitDate: new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc), sphOd: -0.50m);
        var newer = CreateDto(id: newerId, visitDate: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), sphOd: -3.00m);

        _bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
                Arg.Any<GetPatientOpticalPrescriptionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<OpticalPrescriptionHistoryDto> { older, newer });

        // Pass newerId as PrescriptionId1 and olderId as PrescriptionId2 (reversed order)
        var query = new GetPrescriptionComparisonQuery(patientId, newerId, olderId);

        // Act
        var result = await GetPrescriptionComparisonHandler.Handle(
            query, _bus, CancellationToken.None);

        // Assert - regardless of input order, handler should correctly assign older/newer by VisitDate
        result.Should().NotBeNull();
        result!.Older.Id.Should().Be(olderId);
        result.Newer.Id.Should().Be(newerId);
    }
}
