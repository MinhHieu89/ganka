using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// TDD tests for AcceptTechnicianOrder command handler.
/// Tests assignment, concurrency rejection per D-15, and not-found handling.
/// </summary>
public class AcceptTechnicianOrderTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Visit CreateVisitWithPreExamOrder()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.PreExam);
        visit.CreatePreExamOrder();
        return visit;
    }

    [Fact]
    public async Task Handle_AssignsTechnicianSuccessfully()
    {
        var visit = CreateVisitWithPreExamOrder();
        var order = visit.TechnicianOrders.First();
        var techId = Guid.NewGuid();
        var command = new AcceptTechnicianOrderCommand(order.Id, techId, "Tech A");

        _visitRepository.GetByTechnicianOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        var result = await AcceptTechnicianOrderHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.TechnicianId.Should().Be(techId);
        order.TechnicianName.Should().Be("Tech A");
        order.StartedAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenAlreadyAccepted()
    {
        var visit = CreateVisitWithPreExamOrder();
        var order = visit.TechnicianOrders.First();
        order.Accept(Guid.NewGuid(), "Tech A");

        var command = new AcceptTechnicianOrderCommand(order.Id, Guid.NewGuid(), "Tech B");

        _visitRepository.GetByTechnicianOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        var result = await AcceptTechnicianOrderHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Tech A");
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        var command = new AcceptTechnicianOrderCommand(Guid.NewGuid(), Guid.NewGuid(), "Tech A");

        _visitRepository.GetByTechnicianOrderIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns((Visit?)null);

        var result = await AcceptTechnicianOrderHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
