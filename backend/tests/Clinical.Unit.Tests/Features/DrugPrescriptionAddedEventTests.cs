using Clinical.Application.Features;
using Clinical.Contracts.IntegrationEvents;
using Clinical.Domain.Entities;
using Clinical.Domain.Events;
using FluentAssertions;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class DrugPrescriptionAddedEventTests
{
    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private Visit CreateTestVisit(Guid? patientId = null)
    {
        return Visit.Create(
            patientId ?? Guid.NewGuid(),
            "Nguyen Van A",
            Guid.NewGuid(),
            "Dr. Tran",
            new BranchId(DefaultBranchId),
            false);
    }

    private DrugPrescription CreateTestPrescription(Guid visitId)
    {
        var rx = DrugPrescription.Create(visitId, "Take after meals");

        var catalogItem = PrescriptionItem.CreateFromCatalog(
            rx.Id,
            Guid.NewGuid(),
            "Amoxicillin",
            "Amoxicillin",
            "500mg",
            0, 0,
            "1 tablet 3 times/day",
            null,
            6,
            "tablet",
            "3 times/day",
            5,
            false,
            0);
        rx.AddItem(catalogItem);

        var offCatalogItem = PrescriptionItem.CreateOffCatalog(
            rx.Id,
            "Eye Drops Custom",
            null,
            null,
            0, 0,
            "2 drops 4 times/day",
            null,
            2,
            "bottle",
            "4 times/day",
            7,
            false,
            1);
        rx.AddItem(offCatalogItem);

        return rx;
    }

    #region Visit.AddDrugPrescription Domain Event Tests

    [Fact]
    public void AddDrugPrescription_RaisesDrugPrescriptionAddedEvent()
    {
        // Arrange
        var visit = CreateTestVisit();
        visit.ClearDomainEvents(); // Clear VisitCreatedEvent
        var rx = CreateTestPrescription(visit.Id);

        // Act
        visit.AddDrugPrescription(rx);

        // Assert
        visit.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DrugPrescriptionAddedEvent>();
    }

    [Fact]
    public void AddDrugPrescription_Event_HasCorrectVisitId()
    {
        // Arrange
        var visit = CreateTestVisit();
        visit.ClearDomainEvents();
        var rx = CreateTestPrescription(visit.Id);

        // Act
        visit.AddDrugPrescription(rx);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<DrugPrescriptionAddedEvent>().Single();
        domainEvent.VisitId.Should().Be(visit.Id);
    }

    [Fact]
    public void AddDrugPrescription_Event_HasCorrectPatientId()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit = CreateTestVisit(patientId);
        visit.ClearDomainEvents();
        var rx = CreateTestPrescription(visit.Id);

        // Act
        visit.AddDrugPrescription(rx);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<DrugPrescriptionAddedEvent>().Single();
        domainEvent.PatientId.Should().Be(patientId);
    }

    [Fact]
    public void AddDrugPrescription_Event_HasCorrectBranchId()
    {
        // Arrange
        var visit = CreateTestVisit();
        visit.ClearDomainEvents();
        var rx = CreateTestPrescription(visit.Id);

        // Act
        visit.AddDrugPrescription(rx);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<DrugPrescriptionAddedEvent>().Single();
        domainEvent.BranchId.Should().Be(DefaultBranchId);
    }

    [Fact]
    public void AddDrugPrescription_Event_HasCorrectItemCount()
    {
        // Arrange
        var visit = CreateTestVisit();
        visit.ClearDomainEvents();
        var rx = CreateTestPrescription(visit.Id);

        // Act
        visit.AddDrugPrescription(rx);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<DrugPrescriptionAddedEvent>().Single();
        domainEvent.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddDrugPrescription_Event_ItemsHaveCorrectDrugNames()
    {
        // Arrange
        var visit = CreateTestVisit();
        visit.ClearDomainEvents();
        var rx = CreateTestPrescription(visit.Id);

        // Act
        visit.AddDrugPrescription(rx);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<DrugPrescriptionAddedEvent>().Single();
        domainEvent.Items[0].DrugName.Should().Be("Amoxicillin");
        domainEvent.Items[1].DrugName.Should().Be("Eye Drops Custom");
    }

    [Fact]
    public void AddDrugPrescription_Event_CatalogItemHasDrugCatalogItemId()
    {
        // Arrange
        var visit = CreateTestVisit();
        visit.ClearDomainEvents();
        var rx = CreateTestPrescription(visit.Id);

        // Act
        visit.AddDrugPrescription(rx);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<DrugPrescriptionAddedEvent>().Single();
        domainEvent.Items[0].DrugCatalogItemId.Should().NotBeNull();
        domainEvent.Items[1].DrugCatalogItemId.Should().BeNull(); // off-catalog
    }

    [Fact]
    public void AddDrugPrescription_Event_ItemsHaveCorrectQuantities()
    {
        // Arrange
        var visit = CreateTestVisit();
        visit.ClearDomainEvents();
        var rx = CreateTestPrescription(visit.Id);

        // Act
        visit.AddDrugPrescription(rx);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<DrugPrescriptionAddedEvent>().Single();
        domainEvent.Items[0].Quantity.Should().Be(6);
        domainEvent.Items[1].Quantity.Should().Be(2);
    }

    #endregion

    #region Cascading Handler Tests

    [Fact]
    public void CascadingHandler_ConvertsToIntegrationEvent()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var items = new List<DrugPrescriptionAddedEvent.PrescribedDrugDto>
        {
            new("Amoxicillin", Guid.NewGuid(), 6),
            new("Eye Drops Custom", null, 2)
        };
        var domainEvent = new DrugPrescriptionAddedEvent(visitId, patientId, "Nguyen Van A", DefaultBranchId, items);

        // Act
        var integrationEvent = PublishDrugPrescriptionAddedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.Should().NotBeNull();
        integrationEvent.VisitId.Should().Be(visitId);
        integrationEvent.PatientId.Should().Be(patientId);
        integrationEvent.PatientName.Should().Be("Nguyen Van A");
        integrationEvent.BranchId.Should().Be(DefaultBranchId);
    }

    [Fact]
    public void CascadingHandler_MapsAllItems()
    {
        // Arrange
        var catalogItemId = Guid.NewGuid();
        var items = new List<DrugPrescriptionAddedEvent.PrescribedDrugDto>
        {
            new("Amoxicillin", catalogItemId, 6),
            new("Eye Drops Custom", null, 2)
        };
        var domainEvent = new DrugPrescriptionAddedEvent(Guid.NewGuid(), Guid.NewGuid(), "Patient", DefaultBranchId, items);

        // Act
        var integrationEvent = PublishDrugPrescriptionAddedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.Items.Should().HaveCount(2);
        integrationEvent.Items[0].DrugName.Should().Be("Amoxicillin");
        integrationEvent.Items[0].DrugCatalogItemId.Should().Be(catalogItemId);
        integrationEvent.Items[0].Quantity.Should().Be(6);
        integrationEvent.Items[1].DrugName.Should().Be("Eye Drops Custom");
        integrationEvent.Items[1].DrugCatalogItemId.Should().BeNull();
        integrationEvent.Items[1].Quantity.Should().Be(2);
    }

    #endregion
}
