using FluentAssertions;
using NSubstitute;
using Optical.Application.Features.Orders;
using Optical.Application.Interfaces;
using Optical.Contracts.IntegrationEvents;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Optical.Domain.Events;
using Shared.Application;
using Shared.Domain;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// Unit tests for glasses order creation domain event and integration event publishing.
/// Covers: GlassesOrderCreatedEvent raised on order creation,
/// PublishGlassesOrderCreatedIntegrationEventHandler cascading conversion.
/// </summary>
public class GlassesOrderEventTests
{
    private readonly IGlassesOrderRepository _orderRepository = Substitute.For<IGlassesOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultPatientId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid DefaultVisitId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    private static readonly Guid DefaultPrescriptionId = Guid.Parse("00000000-0000-0000-0000-000000000030");

    public GlassesOrderEventTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    // -------------------------------------------------------------------------
    // CreateGlassesOrder raises GlassesOrderCreatedEvent
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateGlassesOrder_RaisesGlassesOrderCreatedEvent_WithCorrectData()
    {
        // Arrange
        GlassesOrder? capturedOrder = null;
        _orderRepository.When(r => r.Add(Arg.Any<GlassesOrder>()))
            .Do(ci => capturedOrder = ci.Arg<GlassesOrder>());

        var command = new CreateGlassesOrderCommand(
            PatientId: DefaultPatientId,
            PatientName: "Nguyen Van A",
            VisitId: DefaultVisitId,
            OpticalPrescriptionId: DefaultPrescriptionId,
            ProcessingType: (int)ProcessingType.InHouse,
            EstimatedDeliveryDate: null,
            TotalPrice: 3_000_000m,
            ComboPackageId: null,
            Notes: null,
            Items:
            [
                new GlassesOrderItemRequest(
                    FrameId: Guid.NewGuid(),
                    LensCatalogItemId: null,
                    Description: "Ray-Ban RB3025",
                    UnitPrice: 1_500_000m,
                    Quantity: 1,
                    DescriptionVi: "Gong kinh Ray-Ban RB3025"),
                new GlassesOrderItemRequest(
                    FrameId: null,
                    LensCatalogItemId: Guid.NewGuid(),
                    Description: "Essilor Crizal",
                    UnitPrice: 750_000m,
                    Quantity: 2,
                    DescriptionVi: "Trong kinh Essilor Crizal")
            ]);

        // Act
        var result = await CreateGlassesOrderHandler.Handle(
            command, _orderRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOrder.Should().NotBeNull();

        var domainEvents = capturedOrder!.DomainEvents;
        domainEvents.Should().ContainSingle(e => e is GlassesOrderCreatedEvent);

        var createdEvent = domainEvents.OfType<GlassesOrderCreatedEvent>().Single();
        createdEvent.OrderId.Should().Be(capturedOrder.Id);
        createdEvent.VisitId.Should().Be(DefaultVisitId);
        createdEvent.PatientId.Should().Be(DefaultPatientId);
        createdEvent.PatientName.Should().Be("Nguyen Van A");
        createdEvent.Items.Should().HaveCount(2);
        createdEvent.Items[0].Description.Should().Be("Ray-Ban RB3025");
        createdEvent.Items[0].DescriptionVi.Should().Be("Gong kinh Ray-Ban RB3025");
        createdEvent.Items[0].UnitPrice.Should().Be(1_500_000m);
        createdEvent.Items[0].Quantity.Should().Be(1);
        createdEvent.Items[1].Description.Should().Be("Essilor Crizal");
        createdEvent.Items[1].DescriptionVi.Should().Be("Trong kinh Essilor Crizal");
        createdEvent.Items[1].UnitPrice.Should().Be(750_000m);
        createdEvent.Items[1].Quantity.Should().Be(2);
    }

    // -------------------------------------------------------------------------
    // PublishGlassesOrderCreatedIntegrationEventHandler converts correctly
    // -------------------------------------------------------------------------

    [Fact]
    public void PublishHandler_ConvertsDomainEventToIntegrationEvent_Correctly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        var domainEvent = new GlassesOrderCreatedEvent(
            OrderId: orderId,
            VisitId: visitId,
            PatientId: patientId,
            PatientName: "Tran Thi B",
            Items:
            [
                new GlassesOrderCreatedEvent.OrderLineDto(
                    Description: "Frame ABC",
                    DescriptionVi: "Gong kinh ABC",
                    UnitPrice: 2_000_000m,
                    Quantity: 1),
                new GlassesOrderCreatedEvent.OrderLineDto(
                    Description: "Lens XYZ",
                    DescriptionVi: "Trong kinh XYZ",
                    UnitPrice: 1_000_000m,
                    Quantity: 2)
            ],
            BranchId: DefaultBranchId);

        // Act
        var integrationEvent = PublishGlassesOrderCreatedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.Should().NotBeNull();
        integrationEvent.OrderId.Should().Be(orderId);
        integrationEvent.VisitId.Should().Be(visitId);
        integrationEvent.PatientId.Should().Be(patientId);
        integrationEvent.PatientName.Should().Be("Tran Thi B");
        integrationEvent.BranchId.Should().Be(DefaultBranchId);
        integrationEvent.Items.Should().HaveCount(2);

        integrationEvent.Items[0].Description.Should().Be("Frame ABC");
        integrationEvent.Items[0].DescriptionVi.Should().Be("Gong kinh ABC");
        integrationEvent.Items[0].UnitPrice.Should().Be(2_000_000m);
        integrationEvent.Items[0].Quantity.Should().Be(1);

        integrationEvent.Items[1].Description.Should().Be("Lens XYZ");
        integrationEvent.Items[1].DescriptionVi.Should().Be("Trong kinh XYZ");
        integrationEvent.Items[1].UnitPrice.Should().Be(1_000_000m);
        integrationEvent.Items[1].Quantity.Should().Be(2);
    }
}
