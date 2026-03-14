using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class GetOsdiAnswersHandlerTests
{
    private readonly IOsdiSubmissionRepository _osdiRepository = Substitute.For<IOsdiSubmissionRepository>();

    [Fact]
    public async Task Handle_SubmissionExists_Returns3GroupsWith12Questions()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var answers = new int?[] { 2, 3, 1, 0, 4, 2, null, 3, 1, 0, 2, 3 };
        var answersJson = System.Text.Json.JsonSerializer.Serialize(answers);
        var submission = OsdiSubmission.Create(visitId, "doctor", answersJson, 11, 45.45m, OsdiSeverity.Severe);

        _osdiRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(submission);

        var query = new GetOsdiAnswersQuery(visitId);

        // Act
        var result = await GetOsdiAnswersHandler.Handle(query, _osdiRepository, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Groups.Should().HaveCount(3);
        result.Groups[0].Category.Should().Be("Ocular Symptoms");
        result.Groups[0].Questions.Should().HaveCount(5);
        result.Groups[1].Category.Should().Be("Vision-Related Function");
        result.Groups[1].Questions.Should().HaveCount(4);
        result.Groups[2].Category.Should().Be("Environmental Triggers");
        result.Groups[2].Questions.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_SubmissionExists_ReturnsCorrectScoresPerQuestion()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var answers = new int?[] { 2, 3, 1, 0, 4, 2, null, 3, 1, 0, 2, 3 };
        var answersJson = System.Text.Json.JsonSerializer.Serialize(answers);
        var submission = OsdiSubmission.Create(visitId, "doctor", answersJson, 11, 45.45m, OsdiSeverity.Severe);

        _osdiRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(submission);

        var query = new GetOsdiAnswersQuery(visitId);

        // Act
        var result = await GetOsdiAnswersHandler.Handle(query, _osdiRepository, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Vision group: Q1=2, Q2=3, Q3=1, Q4=0, Q5=4
        result!.Groups[0].Questions[0].Score.Should().Be(2);
        result.Groups[0].Questions[1].Score.Should().Be(3);
        result.Groups[0].Questions[4].Score.Should().Be(4);
        // Eye symptoms: Q7=null
        result.Groups[1].Questions[1].Score.Should().BeNull();
        result.Groups[1].Questions[1].QuestionNumber.Should().Be(7);
    }

    [Fact]
    public async Task Handle_SubmissionExists_ReturnsTotalScoreAndSeverity()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var answers = new int?[] { 2, 3, 1, 0, 4, 2, null, 3, 1, 0, 2, 3 };
        var answersJson = System.Text.Json.JsonSerializer.Serialize(answers);
        var submission = OsdiSubmission.Create(visitId, "doctor", answersJson, 11, 45.45m, OsdiSeverity.Severe);

        _osdiRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(submission);

        var query = new GetOsdiAnswersQuery(visitId);

        // Act
        var result = await GetOsdiAnswersHandler.Handle(query, _osdiRepository, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TotalScore.Should().Be(45.45m);
        result.Severity.Should().Be("Severe");
    }

    [Fact]
    public async Task Handle_NoSubmission_ReturnsNull()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        _osdiRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>()).Returns((OsdiSubmission?)null);

        var query = new GetOsdiAnswersQuery(visitId);

        // Act
        var result = await GetOsdiAnswersHandler.Handle(query, _osdiRepository, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_QuestionsHaveBilingualText()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var answers = new int?[] { 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2 };
        var answersJson = System.Text.Json.JsonSerializer.Serialize(answers);
        var submission = OsdiSubmission.Create(visitId, "doctor", answersJson, 12, 50m, OsdiSeverity.Severe);

        _osdiRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(submission);

        var query = new GetOsdiAnswersQuery(visitId);

        // Act
        var result = await GetOsdiAnswersHandler.Handle(query, _osdiRepository, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var q1 = result!.Groups[0].Questions[0];
        q1.QuestionNumber.Should().Be(1);
        q1.TextEn.Should().Contain("sensitive to light");
        q1.TextVi.Should().NotBeNullOrEmpty();
    }
}
