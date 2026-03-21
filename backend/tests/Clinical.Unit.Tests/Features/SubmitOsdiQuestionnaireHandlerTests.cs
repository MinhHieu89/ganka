using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class SubmitOsdiQuestionnaireHandlerTests
{
    private readonly IOsdiSubmissionRepository _osdiRepository = Substitute.For<IOsdiSubmissionRepository>();
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IOsdiNotificationService _osdiNotificationService = Substitute.For<IOsdiNotificationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_ValidTokenWithAnswers_CalculatesScoreAndSaves()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var visit = Visit.Create(visitId, "Patient", Guid.NewGuid(), "Dr.", DefaultBranchId, false);
        // Use reflection to set visit Id to match our expected visitId
        typeof(Entity).GetProperty("Id")!.GetSetMethod(true)!.Invoke(visit, [visitId]);

        var submission = OsdiSubmission.CreateWithToken(visitId, "test-token-123");
        _osdiRepository.GetByTokenAsync("test-token-123", Arg.Any<CancellationToken>()).Returns(submission);
        _visitRepository.GetByIdWithDetailsAsync(visitId, Arg.Any<CancellationToken>()).Returns(visit);

        var command = new SubmitOsdiCommand(
            "test-token-123",
            new int?[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 });

        // Act
        var result = await SubmitOsdiQuestionnaireHandler.Handle(
            command, _osdiRepository, _visitRepository, _osdiNotificationService, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var submission = OsdiSubmission.CreateWithToken(Guid.NewGuid(), "expired-token");
        // The token was created just now so it has 24h expiry.
        // We need to simulate expired - set TokenExpiresAt to past
        var tokenExpiresAtProp = typeof(OsdiSubmission).GetProperty("TokenExpiresAt")!;
        tokenExpiresAtProp.GetSetMethod(true)!.Invoke(submission, [DateTime.UtcNow.AddHours(-1)]);

        _osdiRepository.GetByTokenAsync("expired-token", Arg.Any<CancellationToken>()).Returns(submission);

        var command = new SubmitOsdiCommand(
            "expired-token",
            new int?[] { 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2 });

        // Act
        var result = await SubmitOsdiQuestionnaireHandler.Handle(
            command, _osdiRepository, _visitRepository, _osdiNotificationService, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsFailure()
    {
        // Arrange
        _osdiRepository.GetByTokenAsync("invalid-token", Arg.Any<CancellationToken>()).Returns((OsdiSubmission?)null);

        var command = new SubmitOsdiCommand(
            "invalid-token",
            new int?[] { 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2 });

        // Act
        var result = await SubmitOsdiQuestionnaireHandler.Handle(
            command, _osdiRepository, _visitRepository, _osdiNotificationService, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AllNullAnswers_ReturnsFailure()
    {
        // Arrange
        var submission = OsdiSubmission.CreateWithToken(Guid.NewGuid(), "token");
        _osdiRepository.GetByTokenAsync("token", Arg.Any<CancellationToken>()).Returns(submission);

        var command = new SubmitOsdiCommand(
            "token",
            new int?[] { null, null, null, null, null, null, null, null, null, null, null, null });

        // Act
        var result = await SubmitOsdiQuestionnaireHandler.Handle(
            command, _osdiRepository, _visitRepository, _osdiNotificationService, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PartialAnswers_CalculatesScore()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var visit = Visit.Create(visitId, "Patient", Guid.NewGuid(), "Dr.", DefaultBranchId, false);
        typeof(Entity).GetProperty("Id")!.GetSetMethod(true)!.Invoke(visit, [visitId]);

        var submission = OsdiSubmission.CreateWithToken(visitId, "partial-token");
        _osdiRepository.GetByTokenAsync("partial-token", Arg.Any<CancellationToken>()).Returns(submission);
        _visitRepository.GetByIdWithDetailsAsync(visitId, Arg.Any<CancellationToken>()).Returns(visit);

        // Only some questions answered
        var command = new SubmitOsdiCommand(
            "partial-token",
            new int?[] { 2, null, null, null, null, null, null, null, null, null, null, null });

        // Act
        var result = await SubmitOsdiQuestionnaireHandler.Handle(
            command, _osdiRepository, _visitRepository, _osdiNotificationService, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // GenerateOsdiLink tests

    [Fact]
    public async Task GenerateLink_ValidVisit_ReturnsTokenAndUrl()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var visit = Visit.Create(visitId, "Patient", Guid.NewGuid(), "Dr.", DefaultBranchId, false);
        typeof(Entity).GetProperty("Id")!.GetSetMethod(true)!.Invoke(visit, [visitId]);

        _visitRepository.GetByIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(visit);

        var command = new GenerateOsdiLinkCommand(visitId);

        // Act
        var result = await GenerateOsdiLinkHandler.Handle(
            command, _visitRepository, _osdiRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.Url.Should().Contain(result.Value.Token);
        await _osdiRepository.Received(1).AddAsync(Arg.Any<OsdiSubmission>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateLink_VisitNotFound_ReturnsFailure()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        _visitRepository.GetByIdAsync(visitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        var command = new GenerateOsdiLinkCommand(visitId);

        // Act
        var result = await GenerateOsdiLinkHandler.Handle(
            command, _visitRepository, _osdiRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // GetOsdiByToken tests

    [Fact]
    public async Task GetByToken_ValidToken_ReturnsQuestionnaire()
    {
        // Arrange
        var submission = OsdiSubmission.CreateWithToken(Guid.NewGuid(), "valid-token");
        _osdiRepository.GetByTokenAsync("valid-token", Arg.Any<CancellationToken>()).Returns(submission);

        // We need visit for VisitDate
        var visitId = submission.VisitId!.Value;
        var visit = Visit.Create(visitId, "Patient", Guid.NewGuid(), "Dr.", DefaultBranchId, false);
        typeof(Entity).GetProperty("Id")!.GetSetMethod(true)!.Invoke(visit, [visitId]);
        _visitRepository.GetByIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await GetOsdiByTokenHandler.Handle(
            new GetOsdiByTokenQuery("valid-token"), _osdiRepository, _visitRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().HaveCount(12);
        result.Value.Questions.Should().AllSatisfy(q =>
        {
            q.TextEn.Should().NotBeNullOrEmpty();
            q.TextVi.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task GetByToken_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var submission = OsdiSubmission.CreateWithToken(Guid.NewGuid(), "expired-token");
        typeof(OsdiSubmission).GetProperty("TokenExpiresAt")!.GetSetMethod(true)!
            .Invoke(submission, [DateTime.UtcNow.AddHours(-1)]);

        _osdiRepository.GetByTokenAsync("expired-token", Arg.Any<CancellationToken>()).Returns(submission);

        // Act
        var result = await GetOsdiByTokenHandler.Handle(
            new GetOsdiByTokenQuery("expired-token"), _osdiRepository, _visitRepository, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetByToken_InvalidToken_ReturnsFailure()
    {
        // Arrange
        _osdiRepository.GetByTokenAsync("invalid", Arg.Any<CancellationToken>()).Returns((OsdiSubmission?)null);

        // Act
        var result = await GetOsdiByTokenHandler.Handle(
            new GetOsdiByTokenQuery("invalid"), _osdiRepository, _visitRepository, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
