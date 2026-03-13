using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Events;
using Clinical.Domain.Enums;
using Clinical.Contracts.IntegrationEvents;
using FluentAssertions;
using NSubstitute;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class CancelVisitTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private Visit CreateDraftVisit()
    {
        return Visit.Create(
            Guid.NewGuid(),
            "Nguyen Van A",
            Guid.NewGuid(),
            "Dr. Tran",
            new BranchId(DefaultBranchId),
            false);
    }

    // --- Domain tests: Visit.Cancel() ---

    [Fact]
    public void Cancel_DraftVisit_SetsStatusToCancelled()
    {
        // Arrange
        var visit = CreateDraftVisit();
        visit.ClearDomainEvents(); // clear VisitCreatedEvent

        // Act
        visit.Cancel();

        // Assert
        visit.Status.Should().Be(VisitStatus.Cancelled);
    }

    [Fact]
    public void Cancel_DraftVisit_RaisesVisitCancelledEvent()
    {
        // Arrange
        var visit = CreateDraftVisit();
        visit.ClearDomainEvents();

        // Act
        visit.Cancel();

        // Assert
        visit.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VisitCancelledEvent>();
    }

    [Fact]
    public void Cancel_DraftVisit_VisitCancelledEventHasCorrectVisitId()
    {
        // Arrange
        var visit = CreateDraftVisit();
        visit.ClearDomainEvents();

        // Act
        visit.Cancel();

        // Assert
        var domainEvent = visit.DomainEvents.OfType<VisitCancelledEvent>().Single();
        domainEvent.VisitId.Should().Be(visit.Id);
    }

    [Fact]
    public void Cancel_SignedVisit_ThrowsInvalidOperationException()
    {
        // Arrange
        var visit = CreateDraftVisit();
        visit.SignOff(Guid.NewGuid());

        // Act
        var act = () => visit.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Draft*");
    }

    [Fact]
    public void Cancel_AlreadyCancelledVisit_ThrowsInvalidOperationException()
    {
        // Arrange
        var visit = CreateDraftVisit();
        visit.Cancel();

        // Act
        var act = () => visit.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Draft*");
    }

    // --- Handler tests: CancelVisitHandler ---

    [Fact]
    public async Task Handle_ValidDraftVisit_ReturnsSuccess()
    {
        // Arrange
        var visit = CreateDraftVisit();
        var command = new CancelVisitCommand(visit.Id);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        // Act
        var result = await CancelVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidDraftVisit_CallsSaveChanges()
    {
        // Arrange
        var visit = CreateDraftVisit();
        var command = new CancelVisitCommand(visit.Id);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        // Act
        await CancelVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new CancelVisitCommand(Guid.NewGuid());
        _visitRepository.GetByIdAsync(command.VisitId, Arg.Any<CancellationToken>())
            .Returns((Visit?)null);

        // Act
        var result = await CancelVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    // --- Integration event handler tests ---

    [Fact]
    public void PublishVisitCreatedIntegrationEvent_MapsCorrectly()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var domainEvent = new VisitCreatedEvent(visitId, patientId, "Nguyen Van A", doctorId, "Dr. Tran", DefaultBranchId);

        // Act
        var integrationEvent = PublishVisitCreatedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.VisitId.Should().Be(visitId);
        integrationEvent.PatientId.Should().Be(patientId);
        integrationEvent.PatientName.Should().Be("Nguyen Van A");
        integrationEvent.BranchId.Should().Be(DefaultBranchId);
    }

    [Fact]
    public void PublishVisitCancelledIntegrationEvent_MapsCorrectly()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var domainEvent = new VisitCancelledEvent(visitId, DefaultBranchId);

        // Act
        var integrationEvent = PublishVisitCancelledIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.VisitId.Should().Be(visitId);
        integrationEvent.BranchId.Should().Be(DefaultBranchId);
    }
}
