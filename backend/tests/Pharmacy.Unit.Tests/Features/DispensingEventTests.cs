using FluentAssertions;
using Pharmacy.Application.Features.Dispensing;
using Pharmacy.Contracts.IntegrationEvents;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Events;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for drug dispensing domain events and cascading integration event handlers.
/// INT-03: Drug dispensing raises DrugDispensedEvent with per-drug line items.
/// </summary>
public class DispensingEventTests
{
    // =========================================================================
    // DrugDispensedEvent Tests
    // =========================================================================

    [Fact]
    public void DrugDispensedEvent_ShouldContainVisitIdPatientIdAndItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var items = new List<DrugDispensedEvent.DrugLineDto>
        {
            new("Tobramycin 0.3%", "Tobramycin 0,3%", 2, 50000m),
            new("Dexamethasone 0.1%", "Dexamethasone 0,1%", 1, 75000m)
        };

        // Act
        var evt = new DrugDispensedEvent(visitId, patientId, "Nguyen Van A", items, Guid.NewGuid());

        // Assert
        evt.VisitId.Should().Be(visitId);
        evt.PatientId.Should().Be(patientId);
        evt.PatientName.Should().Be("Nguyen Van A");
        evt.Items.Should().HaveCount(2);
        evt.Items[0].DrugName.Should().Be("Tobramycin 0.3%");
        evt.Items[0].DrugNameVi.Should().Be("Tobramycin 0,3%");
        evt.Items[0].Quantity.Should().Be(2);
        evt.Items[0].UnitPrice.Should().Be(50000m);
        evt.EventId.Should().NotBeEmpty();
        evt.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DrugDispensedEvent_ShouldImplementIDomainEvent()
    {
        var evt = new DrugDispensedEvent(Guid.NewGuid(), Guid.NewGuid(), "Test", [], Guid.NewGuid());
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    // =========================================================================
    // DispensingRecord raises DrugDispensedEvent after dispensing
    // =========================================================================

    [Fact]
    public void DispensingRecord_MarkAsDispensed_ShouldRaiseDrugDispensedEvent()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var record = DispensingRecord.Create(
            prescriptionId: Guid.NewGuid(),
            visitId: visitId,
            patientId: patientId,
            patientName: "Nguyen Van A",
            dispensedById: Guid.NewGuid(),
            overrideReason: null,
            branchId: new BranchId(Guid.NewGuid()));

        var items = new List<DrugDispensedEvent.DrugLineDto>
        {
            new("Tobramycin 0.3%", "Tobramycin 0,3%", 2, 50000m)
        };

        // Act
        record.RaiseDispensedEvent(items);

        // Assert
        record.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DrugDispensedEvent>()
            .Which.Should().Match<DrugDispensedEvent>(e =>
                e.VisitId == visitId &&
                e.PatientId == patientId &&
                e.PatientName == "Nguyen Van A" &&
                e.Items.Count == 1);
    }

    // =========================================================================
    // PublishDrugDispensedIntegrationEventHandler Tests
    // =========================================================================

    [Fact]
    public void PublishDrugDispensedIntegrationEventHandler_ShouldConvertDomainEventToIntegrationEvent()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var items = new List<DrugDispensedEvent.DrugLineDto>
        {
            new("Tobramycin 0.3%", "Tobramycin 0,3%", 2, 50000m),
            new("Dexamethasone 0.1%", "Dexamethasone 0,1%", 1, 75000m)
        };
        var domainEvent = new DrugDispensedEvent(visitId, patientId, "Nguyen Van A", items, Guid.NewGuid());

        // Act
        var integrationEvent = PublishDrugDispensedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.Should().NotBeNull();
        integrationEvent.VisitId.Should().Be(visitId);
        integrationEvent.PatientId.Should().Be(patientId);
        integrationEvent.PatientName.Should().Be("Nguyen Van A");
        integrationEvent.Items.Should().HaveCount(2);
        integrationEvent.Items[0].DrugName.Should().Be("Tobramycin 0.3%");
        integrationEvent.Items[0].DrugNameVi.Should().Be("Tobramycin 0,3%");
        integrationEvent.Items[0].Quantity.Should().Be(2);
        integrationEvent.Items[0].UnitPrice.Should().Be(50000m);
        integrationEvent.Items[1].DrugName.Should().Be("Dexamethasone 0.1%");
        integrationEvent.Items[1].Quantity.Should().Be(1);
        integrationEvent.Items[1].UnitPrice.Should().Be(75000m);
    }

    [Fact]
    public void PublishDrugDispensedIntegrationEventHandler_EmptyItems_ShouldReturnEmptyList()
    {
        // Arrange
        var domainEvent = new DrugDispensedEvent(Guid.NewGuid(), Guid.NewGuid(), "Test", [], Guid.NewGuid());

        // Act
        var integrationEvent = PublishDrugDispensedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.Items.Should().BeEmpty();
    }
}
