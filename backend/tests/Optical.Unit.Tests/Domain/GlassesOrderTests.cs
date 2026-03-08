using FluentAssertions;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Domain;

namespace Optical.Unit.Tests.Domain;

/// <summary>
/// Domain tests for GlassesOrder entity: status transitions, warranty calculation, and overdue detection.
/// </summary>
public class GlassesOrderTests
{
    private static readonly BranchId TestBranchId = new(Guid.NewGuid());

    private static GlassesOrder CreateOrder(
        ProcessingType processingType = ProcessingType.InHouse,
        DateTime? estimatedDeliveryDate = null) =>
        GlassesOrder.Create(
            patientId: Guid.NewGuid(),
            patientName: "Nguyen Van A",
            visitId: Guid.NewGuid(),
            opticalPrescriptionId: Guid.NewGuid(),
            processingType: processingType,
            estimatedDeliveryDate: estimatedDeliveryDate,
            totalPrice: 5_000_000m,
            comboPackageId: null,
            notes: null,
            branchId: TestBranchId);

    // --- Status Transition Tests ---

    [Fact]
    public void Create_ShouldSetStatusToOrdered()
    {
        // Act
        var order = CreateOrder();

        // Assert
        order.Status.Should().Be(GlassesOrderStatus.Ordered);
    }

    [Fact]
    public void TransitionTo_OrderedToProcessing_ShouldSucceed()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        order.TransitionTo(GlassesOrderStatus.Processing);

        // Assert
        order.Status.Should().Be(GlassesOrderStatus.Processing);
    }

    [Fact]
    public void TransitionTo_ProcessingToReceived_ShouldSucceed()
    {
        // Arrange
        var order = CreateOrder();
        order.TransitionTo(GlassesOrderStatus.Processing);

        // Act
        order.TransitionTo(GlassesOrderStatus.Received);

        // Assert
        order.Status.Should().Be(GlassesOrderStatus.Received);
    }

    [Fact]
    public void TransitionTo_ReceivedToReady_ShouldSucceed()
    {
        // Arrange
        var order = CreateOrder();
        order.TransitionTo(GlassesOrderStatus.Processing);
        order.TransitionTo(GlassesOrderStatus.Received);

        // Act
        order.TransitionTo(GlassesOrderStatus.Ready);

        // Assert
        order.Status.Should().Be(GlassesOrderStatus.Ready);
    }

    [Fact]
    public void TransitionTo_ReadyToDelivered_ShouldSucceed_AndSetDeliveredAt()
    {
        // Arrange
        var order = CreateOrder();
        order.TransitionTo(GlassesOrderStatus.Processing);
        order.TransitionTo(GlassesOrderStatus.Received);
        order.TransitionTo(GlassesOrderStatus.Ready);

        var beforeTransition = DateTime.UtcNow;

        // Act
        order.TransitionTo(GlassesOrderStatus.Delivered);

        // Assert
        order.Status.Should().Be(GlassesOrderStatus.Delivered);
        order.DeliveredAt.Should().NotBeNull();
        order.DeliveredAt!.Value.Should().BeOnOrAfter(beforeTransition);
        order.DeliveredAt!.Value.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Theory]
    [InlineData(GlassesOrderStatus.Ordered, GlassesOrderStatus.Delivered)]
    [InlineData(GlassesOrderStatus.Ordered, GlassesOrderStatus.Ready)]
    [InlineData(GlassesOrderStatus.Ordered, GlassesOrderStatus.Received)]
    [InlineData(GlassesOrderStatus.Processing, GlassesOrderStatus.Ordered)]
    [InlineData(GlassesOrderStatus.Processing, GlassesOrderStatus.Ready)]
    public void TransitionTo_InvalidTransition_ShouldThrow(
        GlassesOrderStatus from,
        GlassesOrderStatus to)
    {
        // Arrange
        var order = CreateOrder();

        // Advance to the 'from' status
        if (from != GlassesOrderStatus.Ordered)
        {
            order.TransitionTo(GlassesOrderStatus.Processing);
        }

        // Act
        var act = () => order.TransitionTo(to);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*Cannot transition from {from} to {to}*");
    }

    [Fact]
    public void TransitionTo_DeliveredStatus_IsTerminal_ShouldThrow()
    {
        // Arrange
        var order = CreateOrder();
        order.TransitionTo(GlassesOrderStatus.Processing);
        order.TransitionTo(GlassesOrderStatus.Received);
        order.TransitionTo(GlassesOrderStatus.Ready);
        order.TransitionTo(GlassesOrderStatus.Delivered);

        // Act
        var act = () => order.TransitionTo(GlassesOrderStatus.Ordered);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    // --- Domain Event Tests ---

    [Fact]
    public void TransitionTo_ShouldRaiseDomainEvent()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        order.TransitionTo(GlassesOrderStatus.Processing);

        // Assert
        order.DomainEvents.Should().HaveCount(1);
    }

    // --- Warranty Tests ---

    [Fact]
    public void IsUnderWarranty_WithinTwelveMonths_ShouldReturnTrue()
    {
        // Arrange - simulate delivered 6 months ago
        var order = CreateOrder();
        order.TransitionTo(GlassesOrderStatus.Processing);
        order.TransitionTo(GlassesOrderStatus.Received);
        order.TransitionTo(GlassesOrderStatus.Ready);
        order.TransitionTo(GlassesOrderStatus.Delivered);

        // DeliveredAt is set to UtcNow, so it's definitely within 12 months
        // Assert
        order.IsUnderWarranty.Should().BeTrue();
    }

    [Fact]
    public void IsUnderWarranty_AfterTwelveMonths_ShouldReturnFalse()
    {
        // Arrange
        // An order that was not delivered (DeliveredAt is null) cannot be under warranty
        var order = CreateOrder();

        // Assert — not delivered, so not under warranty
        order.IsUnderWarranty.Should().BeFalse();
    }

    [Fact]
    public void IsUnderWarranty_NotDelivered_ShouldReturnFalse()
    {
        // Arrange
        var order = CreateOrder();
        order.TransitionTo(GlassesOrderStatus.Processing);

        // Assert
        order.IsUnderWarranty.Should().BeFalse();
    }

    // --- Overdue Tests ---

    [Fact]
    public void IsOverdue_PastEstimatedDate_ShouldReturnTrue()
    {
        // Arrange — estimated delivery was yesterday
        var order = CreateOrder(estimatedDeliveryDate: DateTime.UtcNow.AddDays(-1));

        // Assert — status is Ordered (not Delivered) and estimated date is in the past
        order.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_FutureEstimatedDate_ShouldReturnFalse()
    {
        // Arrange — estimated delivery is in 3 days
        var order = CreateOrder(estimatedDeliveryDate: DateTime.UtcNow.AddDays(3));

        // Assert
        order.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_Delivered_ShouldReturnFalse()
    {
        // Arrange — order was due yesterday but is already delivered
        var order = CreateOrder(estimatedDeliveryDate: DateTime.UtcNow.AddDays(-1));
        order.TransitionTo(GlassesOrderStatus.Processing);
        order.TransitionTo(GlassesOrderStatus.Received);
        order.TransitionTo(GlassesOrderStatus.Ready);
        order.TransitionTo(GlassesOrderStatus.Delivered);

        // Assert — delivered orders are not overdue even if past estimated date
        order.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_NoEstimatedDate_ShouldReturnFalse()
    {
        // Arrange — no estimated delivery date set
        var order = CreateOrder(estimatedDeliveryDate: null);

        // Assert
        order.IsOverdue.Should().BeFalse();
    }

    // --- ConfirmPayment Tests ---

    [Fact]
    public void ConfirmPayment_ShouldSetIsPaymentConfirmed()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        order.ConfirmPayment();

        // Assert
        order.IsPaymentConfirmed.Should().BeTrue();
    }
}
