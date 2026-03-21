using FluentAssertions;
using NSubstitute;
using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// Tests for GetActiveVisitCountHandler verifying delegation to repository.
/// </summary>
public class GetActiveVisitCountHandlerTests
{
    private readonly IVisitRepository _repo = Substitute.For<IVisitRepository>();

    [Fact]
    public async Task Handle_ReturnsActiveVisitCount_FromRepository()
    {
        // Arrange
        var visits = new List<Visit> { CreateDummyVisit(), CreateDummyVisit(), CreateDummyVisit() };
        _repo.GetActiveVisitsAsync(Arg.Any<CancellationToken>()).Returns(visits);
        var query = new GetActiveVisitCountQuery();

        // Act
        var result = await GetActiveVisitCountHandler.Handle(query, _repo, CancellationToken.None);

        // Assert
        result.Should().Be(3);
        await _repo.Received(1).GetActiveVisitsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsZero_WhenNoActiveVisits()
    {
        // Arrange
        _repo.GetActiveVisitsAsync(Arg.Any<CancellationToken>()).Returns(new List<Visit>());
        var query = new GetActiveVisitCountQuery();

        // Act
        var result = await GetActiveVisitCountHandler.Handle(query, _repo, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    private static Visit CreateDummyVisit()
    {
        return Visit.Create(Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Test Doctor", new Shared.Domain.BranchId(Guid.NewGuid()), false);
    }
}
