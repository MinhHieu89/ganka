using FluentAssertions;
using NSubstitute;
using Patient.Application.Features;
using Patient.Application.Interfaces;

namespace Patient.Unit.Tests.Features;

/// <summary>
/// Tests for GetDashboardStatsHandler verifying that dashboard statistics
/// are correctly returned from the patient repository.
/// </summary>
public class GetDashboardStatsHandlerTests
{
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();

    [Fact]
    public async Task Handle_ReturnsActivePatientCount()
    {
        // Arrange
        _patientRepository.GetActiveCountAsync(Arg.Any<CancellationToken>()).Returns(42);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await GetDashboardStatsHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPatients.Should().Be(42);
        await _patientRepository.Received(1).GetActiveCountAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsZeroWhenNoPatientsExist()
    {
        // Arrange
        _patientRepository.GetActiveCountAsync(Arg.Any<CancellationToken>()).Returns(0);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await GetDashboardStatsHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPatients.Should().Be(0);
    }
}
