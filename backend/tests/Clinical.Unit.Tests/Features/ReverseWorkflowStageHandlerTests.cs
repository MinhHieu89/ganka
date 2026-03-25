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

/// <summary>
/// Tests for ReverseWorkflowStageHandler.
/// Validates valid/invalid reversals, not-found, and empty reason.
/// </summary>
public class ReverseWorkflowStageHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_ValidReversal_ReturnsSuccessAndUpdatesStage()
    {
        // Arrange -- visit at RefractionVA, reverse to Reception
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.RefractionVA);

        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        var command = new ReverseWorkflowStageCommand(visit.Id, (int)WorkflowStage.Reception, "Wrong patient");

        // Act
        var result = await ReverseWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.Reception);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _visitRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Visit?)null);

        var command = new ReverseWorkflowStageCommand(Guid.NewGuid(), 0, "Some reason");

        // Act
        var result = await ReverseWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_InvalidReversal_CashierToDoctorExam_ReturnsValidationError()
    {
        // Arrange -- visit at Cashier (no-imaging path)
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient B", Guid.NewGuid(), "Dr. B",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.RefractionVA);
        visit.AdvanceStage(WorkflowStage.DoctorExam);
        visit.AdvanceStage(WorkflowStage.Prescription);
        visit.AdvanceStage(WorkflowStage.Cashier);

        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        var command = new ReverseWorkflowStageCommand(visit.Id, (int)WorkflowStage.DoctorExam, "Need re-exam");

        // Act
        var result = await ReverseWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_EmptyReason_ReturnsValidationError()
    {
        // Arrange
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient C", Guid.NewGuid(), "Dr. C",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.RefractionVA);

        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        var command = new ReverseWorkflowStageCommand(visit.Id, (int)WorkflowStage.Reception, "");

        // Act
        var result = await ReverseWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }
}
