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

public class SkipRefractionHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public SkipRefractionHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.Email.Returns("tech@ganka28.com");
    }

    private static Visit CreateVisitAtStage(WorkflowStage stage)
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);

        WorkflowStage[] path = [WorkflowStage.RefractionVA, WorkflowStage.DoctorExam,
            WorkflowStage.Prescription, WorkflowStage.Cashier];

        foreach (var s in path)
        {
            visit.AdvanceStage(s);
            if (s == stage) break;
        }

        return visit;
    }

    [Fact]
    public async Task Handle_AtRefractionVA_SkipsAndAdvancesToDoctorExam()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.RefractionVA);
        var command = new SkipRefractionCommand(visit.Id, (int)SkipReason.FollowUpExisting, null);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SkipRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.RefractionSkipped.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.DoctorExam);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotAtRefractionVA_ReturnsError()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        var command = new SkipRefractionCommand(visit.Id, (int)SkipReason.FollowUpExisting, null);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SkipRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new SkipRefractionCommand(Guid.NewGuid(), (int)SkipReason.Other, "test");
        _visitRepository.GetByIdAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await SkipRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WithFreeTextNote_CreatesStageSkipRecord()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.RefractionVA);
        var command = new SkipRefractionCommand(visit.Id, (int)SkipReason.Other, "Patient came for OTC only");
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SkipRefractionHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.StageSkips.Should().HaveCount(1);
        visit.StageSkips.First().FreeTextNote.Should().Be("Patient came for OTC only");
    }
}
