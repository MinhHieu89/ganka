using FluentAssertions;
using NSubstitute;
using Treatment.Application.Features;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Queries;
using Treatment.Domain.Entities;

namespace Treatment.Unit.Tests.Features;

/// <summary>
/// Tests for GetActiveTreatmentCountHandler verifying delegation to repository.
/// </summary>
public class GetActiveTreatmentCountHandlerTests
{
    private readonly ITreatmentPackageRepository _repo = Substitute.For<ITreatmentPackageRepository>();

    [Fact]
    public async Task Handle_ReturnsActiveTreatmentCount_FromRepository()
    {
        // Arrange
        var packages = new List<TreatmentPackage>
        {
            CreateDummyPackage(),
            CreateDummyPackage()
        };
        _repo.GetActivePackagesAsync(Arg.Any<CancellationToken>()).Returns(packages);
        var query = new GetActiveTreatmentCountQuery();

        // Act
        var result = await GetActiveTreatmentCountHandler.Handle(query, _repo, CancellationToken.None);

        // Assert
        result.Should().Be(2);
        await _repo.Received(1).GetActivePackagesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsZero_WhenNoActivePackages()
    {
        // Arrange
        _repo.GetActivePackagesAsync(Arg.Any<CancellationToken>()).Returns(new List<TreatmentPackage>());
        var query = new GetActiveTreatmentCountQuery();

        // Act
        var result = await GetActiveTreatmentCountHandler.Handle(query, _repo, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    private static TreatmentPackage CreateDummyPackage()
    {
        return TreatmentPackage.Create(
            protocolTemplateId: Guid.NewGuid(),
            patientId: Guid.NewGuid(),
            patientName: "Test Patient",
            treatmentType: Treatment.Domain.Enums.TreatmentType.IPL,
            totalSessions: 3,
            pricingMode: Treatment.Domain.Enums.PricingMode.PerSession,
            packagePrice: 0m,
            sessionPrice: 100m,
            minIntervalDays: 7,
            parametersJson: "{}",
            visitId: null,
            createdById: Guid.NewGuid(),
            branchId: new Shared.Domain.BranchId(Guid.NewGuid()));
    }
}
