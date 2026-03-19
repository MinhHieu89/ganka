using Clinical.Domain.Entities;
using FluentAssertions;

namespace Clinical.Unit.Tests.Domain;

/// <summary>
/// Tests for OsdiSubmission domain entity factory methods.
/// </summary>
public class OsdiSubmissionDomainTests
{
    [Fact]
    public void CreateWithTokenForTreatment_SetsNullVisitId()
    {
        // Act
        var submission = OsdiSubmission.CreateWithTokenForTreatment("test-token");

        // Assert
        submission.VisitId.Should().BeNull();
        submission.PublicToken.Should().Be("test-token");
        submission.SubmittedBy.Should().Be("patient");
        submission.TokenExpiresAt.Should().NotBeNull();
        submission.TokenExpiresAt!.Value.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CreateWithToken_StillSetsVisitId()
    {
        // Arrange
        var visitId = Guid.NewGuid();

        // Act
        var submission = OsdiSubmission.CreateWithToken(visitId, "visit-token");

        // Assert
        submission.VisitId.Should().Be(visitId);
        submission.PublicToken.Should().Be("visit-token");
        submission.SubmittedBy.Should().Be("patient");
    }
}
