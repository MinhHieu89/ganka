using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.IntegrationEvents;
using Clinical.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// Tests for CreateOsdiTokenForTreatmentHandler - creates DB-backed OSDI tokens for treatment sessions.
/// </summary>
public class CreateOsdiTokenForTreatmentHandlerTests
{
    private readonly IOsdiSubmissionRepository _osdiRepository = Substitute.For<IOsdiSubmissionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ValidToken_CreatesOsdiSubmissionWithNullVisitId()
    {
        // Arrange
        var token = "test-token-abc";
        var command = new CreateOsdiTokenForTreatmentCommand(token);
        OsdiSubmission? savedSubmission = null;
        _osdiRepository.AddAsync(Arg.Do<OsdiSubmission>(s => savedSubmission = s), Arg.Any<CancellationToken>());

        // Act
        var response = await CreateOsdiTokenForTreatmentHandler.Handle(
            command, _osdiRepository, _unitOfWork, CancellationToken.None);

        // Assert
        response.Token.Should().Be(token);
        response.Url.Should().Be($"/osdi/{token}");
        response.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));

        savedSubmission.Should().NotBeNull();
        savedSubmission!.VisitId.Should().BeNull();
        savedSubmission.PublicToken.Should().Be(token);
        savedSubmission.SubmittedBy.Should().Be("patient");
        savedSubmission.TokenExpiresAt.Should().NotBeNull();

        await _osdiRepository.Received(1).AddAsync(Arg.Any<OsdiSubmission>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsCorrectUrl()
    {
        // Arrange
        var token = "my-unique-token";
        var command = new CreateOsdiTokenForTreatmentCommand(token);

        // Act
        var response = await CreateOsdiTokenForTreatmentHandler.Handle(
            command, _osdiRepository, _unitOfWork, CancellationToken.None);

        // Assert
        response.Url.Should().Be("/osdi/my-unique-token");
    }
}
