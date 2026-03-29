using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using Shared.Domain;

namespace Clinical.Unit.Tests.Domain;

/// <summary>
/// TDD tests for TechnicianOrder entity domain logic.
/// </summary>
public class TechnicianOrderTests
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public void CreatePreExam_SetsOrderTypeAndOrderedAt()
    {
        var visitId = Guid.NewGuid();

        var order = TechnicianOrder.CreatePreExam(visitId);

        order.VisitId.Should().Be(visitId);
        order.OrderType.Should().Be(TechnicianOrderType.PreExam);
        order.OrderedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.TechnicianId.Should().BeNull();
        order.TechnicianName.Should().BeNull();
        order.StartedAt.Should().BeNull();
        order.CompletedAt.Should().BeNull();
        order.IsRedFlag.Should().BeFalse();
    }

    [Fact]
    public void Accept_SetsTechnicianIdNameAndStartedAt()
    {
        var order = TechnicianOrder.CreatePreExam(Guid.NewGuid());
        var techId = Guid.NewGuid();

        order.Accept(techId, "Tech A");

        order.TechnicianId.Should().Be(techId);
        order.TechnicianName.Should().Be("Tech A");
        order.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Accept_WhenAlreadyAccepted_ThrowsInvalidOperation()
    {
        var order = TechnicianOrder.CreatePreExam(Guid.NewGuid());
        order.Accept(Guid.NewGuid(), "Tech A");

        var act = () => order.Accept(Guid.NewGuid(), "Tech B");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Tech A*");
    }

    [Fact]
    public void Complete_SetsCompletedAt()
    {
        var order = TechnicianOrder.CreatePreExam(Guid.NewGuid());
        order.Accept(Guid.NewGuid(), "Tech A");

        order.Complete();

        order.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_WhenNotAccepted_ThrowsInvalidOperation()
    {
        var order = TechnicianOrder.CreatePreExam(Guid.NewGuid());

        var act = () => order.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReturnToQueue_ClearsTechnicianFields()
    {
        var order = TechnicianOrder.CreatePreExam(Guid.NewGuid());
        order.Accept(Guid.NewGuid(), "Tech A");

        order.ReturnToQueue();

        order.TechnicianId.Should().BeNull();
        order.TechnicianName.Should().BeNull();
        order.StartedAt.Should().BeNull();
    }

    [Fact]
    public void MarkRedFlag_SetsRedFlagFieldsAndCompletedAt()
    {
        var order = TechnicianOrder.CreatePreExam(Guid.NewGuid());
        order.Accept(Guid.NewGuid(), "Tech A");

        order.MarkRedFlag("Patient uncooperative");

        order.IsRedFlag.Should().BeTrue();
        order.RedFlagReason.Should().Be("Patient uncooperative");
        order.RedFlaggedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ===================== Visit.CreatePreExamOrder Tests =====================

    [Fact]
    public void Visit_CreatePreExamOrder_AddsTechnicianOrder()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);

        var order = visit.CreatePreExamOrder();

        order.Should().NotBeNull();
        order.VisitId.Should().Be(visit.Id);
        order.OrderType.Should().Be(TechnicianOrderType.PreExam);
        visit.TechnicianOrders.Should().HaveCount(1);
    }

    [Fact]
    public void Visit_CreatePreExamOrder_ThrowsIfAlreadyExists()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        visit.CreatePreExamOrder();

        var act = () => visit.CreatePreExamOrder();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
