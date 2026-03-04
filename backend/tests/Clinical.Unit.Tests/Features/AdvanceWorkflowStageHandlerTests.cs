using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class AdvanceWorkflowStageHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Visit CreateVisitAtStage(WorkflowStage stage = WorkflowStage.Reception)
    {
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);

        // Advance to the desired stage sequentially
        for (var s = WorkflowStage.Reception + 1; s <= stage; s++)
            visit.AdvanceStage(s);

        return visit;
    }

    [Fact]
    public async Task Handle_ReceptionToRefractionVA_StageUpdated()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.Reception);
        var command = new AdvanceWorkflowStageCommand(visit.Id, (int)WorkflowStage.RefractionVA);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AdvanceWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.RefractionVA);
    }

    [Fact]
    public async Task Handle_DoctorExamToDiagnostics_StageUpdated()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        var command = new AdvanceWorkflowStageCommand(visit.Id, (int)WorkflowStage.Diagnostics);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AdvanceWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.Diagnostics);
    }

    [Fact]
    public async Task Handle_InvalidStageTransition_ReturnsError()
    {
        // Arrange -- try to go backwards from DoctorExam to Reception
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        var command = new AdvanceWorkflowStageCommand(visit.Id, (int)WorkflowStage.Reception);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AdvanceWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new AdvanceWorkflowStageCommand(Guid.NewGuid(), (int)WorkflowStage.RefractionVA);
        _visitRepository.GetByIdAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await AdvanceWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_ValidAdvance_SaveChangesCalled()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.Reception);
        var command = new AdvanceWorkflowStageCommand(visit.Id, (int)WorkflowStage.RefractionVA);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        await AdvanceWorkflowStageHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
