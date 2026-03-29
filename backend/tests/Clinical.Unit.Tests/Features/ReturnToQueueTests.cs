using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// TDD tests for ReturnToQueue command handler.
/// Tests clearing technician assignment without changing visit stage.
/// </summary>
public class ReturnToQueueTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_ClearsTechnicianAssignment()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.PreExam);
        visit.CreatePreExamOrder();
        var order = visit.TechnicianOrders.First();
        order.Accept(Guid.NewGuid(), "Tech A");

        var command = new ReturnToQueueCommand(order.Id);

        _visitRepository.GetByTechnicianOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        var result = await ReturnToQueueHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.TechnicianId.Should().BeNull();
        order.TechnicianName.Should().BeNull();
        order.StartedAt.Should().BeNull();
        visit.CurrentStage.Should().Be(WorkflowStage.PreExam); // Stage unchanged
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        var command = new ReturnToQueueCommand(Guid.NewGuid());

        _visitRepository.GetByTechnicianOrderIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns((Visit?)null);

        var result = await ReturnToQueueHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
