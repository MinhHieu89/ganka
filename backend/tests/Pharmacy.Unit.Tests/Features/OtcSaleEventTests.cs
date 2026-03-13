using FluentAssertions;
using Pharmacy.Application.Features.OtcSales;
using Pharmacy.Contracts.IntegrationEvents;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Events;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for OTC sale domain events and cascading integration event handlers.
/// INT-04: OTC sale raises OtcSaleCompletedEvent with sale items and customer info.
/// </summary>
public class OtcSaleEventTests
{
    // =========================================================================
    // OtcSaleCompletedEvent Tests
    // =========================================================================

    [Fact]
    public void OtcSaleCompletedEvent_ShouldContainSaleIdAndItems()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var items = new List<OtcSaleCompletedEvent.DrugLineDto>
        {
            new("Artificial Tears", "Nuoc mat nhan tao", 3, 35000m),
            new("Vitamin A Eye Drops", "Nho mat Vitamin A", 1, 45000m)
        };

        // Act
        var evt = new OtcSaleCompletedEvent(saleId, patientId, "Tran Thi B", items);

        // Assert
        evt.OtcSaleId.Should().Be(saleId);
        evt.PatientId.Should().Be(patientId);
        evt.CustomerName.Should().Be("Tran Thi B");
        evt.Items.Should().HaveCount(2);
        evt.Items[0].DrugName.Should().Be("Artificial Tears");
        evt.Items[0].DrugNameVi.Should().Be("Nuoc mat nhan tao");
        evt.Items[0].Quantity.Should().Be(3);
        evt.Items[0].UnitPrice.Should().Be(35000m);
        evt.EventId.Should().NotBeEmpty();
        evt.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void OtcSaleCompletedEvent_NullPatientId_ShouldBeAllowed()
    {
        // Arrange & Act
        var evt = new OtcSaleCompletedEvent(Guid.NewGuid(), null, "Anonymous", []);

        // Assert
        evt.PatientId.Should().BeNull();
        evt.CustomerName.Should().Be("Anonymous");
    }

    [Fact]
    public void OtcSaleCompletedEvent_ShouldImplementIDomainEvent()
    {
        var evt = new OtcSaleCompletedEvent(Guid.NewGuid(), null, null, []);
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    // =========================================================================
    // OtcSale raises OtcSaleCompletedEvent after sale
    // =========================================================================

    [Fact]
    public void OtcSale_RaiseSaleCompletedEvent_ShouldRaiseOtcSaleCompletedEvent()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var sale = OtcSale.Create(
            patientId: patientId,
            customerName: "Tran Thi B",
            soldById: Guid.NewGuid(),
            notes: null,
            branchId: new BranchId(Guid.NewGuid()));

        var items = new List<OtcSaleCompletedEvent.DrugLineDto>
        {
            new("Artificial Tears", "Nuoc mat nhan tao", 3, 35000m)
        };

        // Act
        sale.RaiseSaleCompletedEvent(items);

        // Assert
        sale.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OtcSaleCompletedEvent>()
            .Which.Should().Match<OtcSaleCompletedEvent>(e =>
                e.OtcSaleId == sale.Id &&
                e.PatientId == patientId &&
                e.CustomerName == "Tran Thi B" &&
                e.Items.Count == 1);
    }

    // =========================================================================
    // PublishOtcSaleCompletedIntegrationEventHandler Tests
    // =========================================================================

    [Fact]
    public void PublishOtcSaleCompletedIntegrationEventHandler_ShouldConvertDomainEventToIntegrationEvent()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var items = new List<OtcSaleCompletedEvent.DrugLineDto>
        {
            new("Artificial Tears", "Nuoc mat nhan tao", 3, 35000m),
            new("Vitamin A Eye Drops", "Nho mat Vitamin A", 1, 45000m)
        };
        var domainEvent = new OtcSaleCompletedEvent(saleId, patientId, "Tran Thi B", items);

        // Act
        var integrationEvent = PublishOtcSaleCompletedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.Should().NotBeNull();
        integrationEvent.OtcSaleId.Should().Be(saleId);
        integrationEvent.PatientId.Should().Be(patientId);
        integrationEvent.CustomerName.Should().Be("Tran Thi B");
        integrationEvent.Items.Should().HaveCount(2);
        integrationEvent.Items[0].DrugName.Should().Be("Artificial Tears");
        integrationEvent.Items[0].DrugNameVi.Should().Be("Nuoc mat nhan tao");
        integrationEvent.Items[0].Quantity.Should().Be(3);
        integrationEvent.Items[0].UnitPrice.Should().Be(35000m);
    }

    [Fact]
    public void PublishOtcSaleCompletedIntegrationEventHandler_NullPatient_ShouldMapCorrectly()
    {
        // Arrange
        var domainEvent = new OtcSaleCompletedEvent(Guid.NewGuid(), null, null, []);

        // Act
        var integrationEvent = PublishOtcSaleCompletedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.PatientId.Should().BeNull();
        integrationEvent.CustomerName.Should().BeNull();
        integrationEvent.Items.Should().BeEmpty();
    }
}
