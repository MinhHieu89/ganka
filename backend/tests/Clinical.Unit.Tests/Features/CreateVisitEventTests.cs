using Clinical.Domain.Entities;
using Clinical.Domain.Events;
using Clinical.Domain.Enums;
using FluentAssertions;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class CreateVisitEventTests
{
    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Fact]
    public void Create_RaisesVisitCreatedEvent()
    {
        // Act
        var visit = Visit.Create(
            Guid.NewGuid(),
            "Nguyen Van A",
            Guid.NewGuid(),
            "Dr. Tran",
            new BranchId(DefaultBranchId),
            false);

        // Assert
        visit.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VisitCreatedEvent>();
    }

    [Fact]
    public void Create_VisitCreatedEvent_HasCorrectPatientId()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        // Act
        var visit = Visit.Create(
            patientId,
            "Nguyen Van A",
            Guid.NewGuid(),
            "Dr. Tran",
            new BranchId(DefaultBranchId),
            false);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<VisitCreatedEvent>().Single();
        domainEvent.PatientId.Should().Be(patientId);
    }

    [Fact]
    public void Create_VisitCreatedEvent_HasCorrectPatientName()
    {
        // Act
        var visit = Visit.Create(
            Guid.NewGuid(),
            "Nguyen Van A",
            Guid.NewGuid(),
            "Dr. Tran",
            new BranchId(DefaultBranchId),
            false);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<VisitCreatedEvent>().Single();
        domainEvent.PatientName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public void Create_VisitCreatedEvent_HasCorrectDoctorId()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        // Act
        var visit = Visit.Create(
            Guid.NewGuid(),
            "Nguyen Van A",
            doctorId,
            "Dr. Tran",
            new BranchId(DefaultBranchId),
            false);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<VisitCreatedEvent>().Single();
        domainEvent.DoctorId.Should().Be(doctorId);
    }

    [Fact]
    public void Create_VisitCreatedEvent_HasCorrectVisitId()
    {
        // Act
        var visit = Visit.Create(
            Guid.NewGuid(),
            "Nguyen Van A",
            Guid.NewGuid(),
            "Dr. Tran",
            new BranchId(DefaultBranchId),
            false);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<VisitCreatedEvent>().Single();
        domainEvent.VisitId.Should().Be(visit.Id);
    }

    [Fact]
    public void Create_VisitCreatedEvent_HasCorrectDoctorName()
    {
        // Act
        var visit = Visit.Create(
            Guid.NewGuid(),
            "Nguyen Van A",
            Guid.NewGuid(),
            "Dr. Tran",
            new BranchId(DefaultBranchId),
            false);

        // Assert
        var domainEvent = visit.DomainEvents.OfType<VisitCreatedEvent>().Single();
        domainEvent.DoctorName.Should().Be("Dr. Tran");
    }
}
