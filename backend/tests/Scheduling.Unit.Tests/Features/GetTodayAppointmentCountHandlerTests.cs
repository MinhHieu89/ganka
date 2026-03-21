using FluentAssertions;
using NSubstitute;
using Scheduling.Application.Features;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Queries;

namespace Scheduling.Unit.Tests.Features;

/// <summary>
/// Tests for GetTodayAppointmentCountHandler verifying delegation to repository.
/// </summary>
public class GetTodayAppointmentCountHandlerTests
{
    private readonly IAppointmentRepository _repo = Substitute.For<IAppointmentRepository>();

    [Fact]
    public async Task Handle_ReturnsTodayCount_FromRepository()
    {
        // Arrange
        _repo.GetTodayCountAsync(Arg.Any<CancellationToken>()).Returns(7);
        var query = new GetTodayAppointmentCountQuery();

        // Act
        var result = await GetTodayAppointmentCountHandler.Handle(query, _repo, CancellationToken.None);

        // Assert
        result.Should().Be(7);
        await _repo.Received(1).GetTodayCountAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsZero_WhenNoAppointmentsToday()
    {
        // Arrange
        _repo.GetTodayCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        var query = new GetTodayAppointmentCountQuery();

        // Act
        var result = await GetTodayAppointmentCountHandler.Handle(query, _repo, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }
}
