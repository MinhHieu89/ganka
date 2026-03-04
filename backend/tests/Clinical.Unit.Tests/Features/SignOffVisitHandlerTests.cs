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

    [Fact]
    public async Task Handle_DraftVisit_StatusBecomesSigned()
    {
        // Arrange
        var visit = CreateDraftVisit();
        var command = new SignOffVisitCommand(visit.Id);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

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
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

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
        _visitRepository.GetByIdAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

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
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

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
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        var before = DateTime.UtcNow;

        // Act
        await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        visit.SignedAt.Should().BeOnOrAfter(before);
        visit.SignedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }
}
