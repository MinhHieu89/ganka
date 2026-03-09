using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class SignOffVisitHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DoctorId = Guid.NewGuid();
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public SignOffVisitHandlerTests()
    {
        _currentUser.UserId.Returns(DoctorId);
    }

    private static Visit CreateDraftVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
    }

    private static Visit CreateAmendedVisit(out VisitAmendment amendment)
    {
        var visit = CreateDraftVisit();
        visit.SignOff(Guid.NewGuid());

        var baselineJson = "{\"examinationNotes\":\"old notes\"}";
        amendment = VisitAmendment.Create(
            visit.Id, Guid.NewGuid(), "Dr. A", "Correcting notes", baselineJson);
        visit.StartAmendment(amendment);

        return visit;
    }

    [Fact]
    public async Task Handle_DraftVisit_StatusBecomesSigned()
    {
        // Arrange
        var visit = CreateDraftVisit();
        var command = new SignOffVisitCommand(visit.Id);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Status.Should().Be(VisitStatus.Signed);
        visit.SignedAt.Should().NotBeNull();
        visit.SignedById.Should().Be(DoctorId);
    }

    [Fact]
    public async Task Handle_AlreadySignedVisit_ReturnsError()
    {
        // Arrange
        var visit = CreateDraftVisit();
        visit.SignOff(Guid.NewGuid()); // Already signed
        var command = new SignOffVisitCommand(visit.Id);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new SignOffVisitCommand(Guid.NewGuid());
        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_DraftVisit_SaveChangesCalled()
    {
        // Arrange
        var visit = CreateDraftVisit();
        var command = new SignOffVisitCommand(visit.Id);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SignedVisit_SignedAtIsRecent()
    {
        // Arrange
        var visit = CreateDraftVisit();
        var command = new SignOffVisitCommand(visit.Id);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        var before = DateTime.UtcNow;

        // Act
        await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        visit.SignedAt.Should().BeOnOrAfter(before);
        visit.SignedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_AmendedVisitWithFieldChanges_UpdatesLatestAmendment()
    {
        // Arrange
        var visit = CreateAmendedVisit(out var amendment);
        var actualDiff = "[{\"field\":\"examinationNotes\",\"oldValue\":\"old notes\",\"newValue\":\"new notes\"}]";
        var command = new SignOffVisitCommand(visit.Id, actualDiff);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Status.Should().Be(VisitStatus.Signed);
        amendment.FieldChangesJson.Should().Be(actualDiff);
    }

    [Fact]
    public async Task Handle_AmendedVisitWithoutFieldChanges_DoesNotUpdateAmendment()
    {
        // Arrange
        var visit = CreateAmendedVisit(out var amendment);
        var originalFieldChanges = amendment.FieldChangesJson;
        var command = new SignOffVisitCommand(visit.Id); // No FieldChangesJson
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Status.Should().Be(VisitStatus.Signed);
        amendment.FieldChangesJson.Should().Be(originalFieldChanges);
    }

    [Fact]
    public async Task Handle_DraftVisitWithFieldChanges_IgnoresFieldChanges()
    {
        // Arrange - Draft visit (not amended), but FieldChangesJson provided
        var visit = CreateDraftVisit();
        var command = new SignOffVisitCommand(visit.Id, "[{\"field\":\"test\"}]");
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Status.Should().Be(VisitStatus.Signed);
        // No amendments to update, should just succeed
        visit.Amendments.Should().BeEmpty();
    }
}
