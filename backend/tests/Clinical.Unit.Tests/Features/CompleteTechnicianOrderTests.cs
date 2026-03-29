using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// TDD tests for CompleteTechnicianOrder command handler.
/// Tests completion with visit stage advancement to DoctorExam.
/// </summary>
public class CompleteTechnicianOrderTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Visit CreateVisitWithAcceptedOrder()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.PreExam);
        visit.CreatePreExamOrder();
        visit.TechnicianOrders.First().Accept(Guid.NewGuid(), "Tech A");
        return visit;
    }

    [Fact]
    public async Task Handle_CompletesOrderAndAdvancesVisitToDoctorExam()
    {
        var visit = CreateVisitWithAcceptedOrder();
        var order = visit.TechnicianOrders.First();
        var command = new CompleteTechnicianOrderCommand(order.Id);

        _visitRepository.GetByTechnicianOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        var result = await CompleteTechnicianOrderHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.CompletedAt.Should().NotBeNull();
        visit.CurrentStage.Should().Be(WorkflowStage.DoctorExam);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenOrderNotAccepted()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.PreExam);
        visit.CreatePreExamOrder();
        var order = visit.TechnicianOrders.First();
        var command = new CompleteTechnicianOrderCommand(order.Id);

        _visitRepository.GetByTechnicianOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        var result = await CompleteTechnicianOrderHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        var command = new CompleteTechnicianOrderCommand(Guid.NewGuid());

        _visitRepository.GetByTechnicianOrderIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns((Visit?)null);

        var result = await CompleteTechnicianOrderHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
