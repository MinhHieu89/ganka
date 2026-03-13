using FluentAssertions;
using Shared.Domain;
using Treatment.Application.Features;
using Treatment.Domain.Entities;
using Treatment.Domain.Enums;
using Treatment.Domain.Events;

namespace Treatment.Unit.Tests.Features;

/// <summary>
/// TDD tests for extended TreatmentSessionCompletedEvent with VisitId and SessionFeeAmount.
/// INT-06: TreatmentSessionCompletedIntegrationEvent includes VisitId and SessionFeeAmount for billing.
/// </summary>
public class SessionCompletedEventTests
{
    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid DefaultPatientId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid DefaultProtocolId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    private static readonly Guid DefaultVisitId = Guid.Parse("00000000-0000-0000-0000-000000000005");

    private TreatmentPackage CreatePackage(
        PricingMode pricingMode = PricingMode.PerPackage,
        decimal packagePrice = 4_000_000m,
        decimal sessionPrice = 1_200_000m,
        int totalSessions = 4,
        Guid? visitId = null)
    {
        return TreatmentPackage.Create(
            protocolTemplateId: DefaultProtocolId,
            patientId: DefaultPatientId,
            patientName: "Nguyen Van A",
            treatmentType: TreatmentType.IPL,
            totalSessions: totalSessions,
            pricingMode: pricingMode,
            packagePrice: packagePrice,
            sessionPrice: sessionPrice,
            minIntervalDays: 14,
            parametersJson: "{\"energy\":15}",
            visitId: visitId,
            createdById: DefaultUserId,
            branchId: new BranchId(DefaultBranchId));
    }

    // =========================================================================
    // TreatmentSessionCompletedEvent includes VisitId and SessionFeeAmount
    // =========================================================================

    [Fact]
    public void SessionCompletedEvent_ShouldIncludeVisitIdAndSessionFeeAmount()
    {
        // Arrange
        var package = CreatePackage(
            pricingMode: PricingMode.PerSession,
            sessionPrice: 1_200_000m,
            visitId: DefaultVisitId);

        // Act
        package.RecordSession(
            "{\"energy\":15}", null, null, null,
            DefaultUserId, null, null, null, []);

        // Assert
        var evt = package.DomainEvents
            .OfType<TreatmentSessionCompletedEvent>()
            .First();

        evt.VisitId.Should().Be(DefaultVisitId);
        evt.SessionFeeAmount.Should().Be(1_200_000m);
    }

    [Fact]
    public void SessionCompletedEvent_PerSession_SessionFeeAmount_EqualsSessionPrice()
    {
        // Arrange
        var package = CreatePackage(
            pricingMode: PricingMode.PerSession,
            sessionPrice: 1_500_000m,
            visitId: DefaultVisitId);

        // Act
        package.RecordSession(
            "{\"energy\":15}", null, null, null,
            DefaultUserId, null, null, null, []);

        // Assert
        var evt = package.DomainEvents
            .OfType<TreatmentSessionCompletedEvent>()
            .First();

        evt.SessionFeeAmount.Should().Be(1_500_000m);
    }

    [Fact]
    public void SessionCompletedEvent_PerPackage_SessionFeeAmount_EqualsPackagePriceDividedByTotalSessions()
    {
        // Arrange
        var package = CreatePackage(
            pricingMode: PricingMode.PerPackage,
            packagePrice: 4_000_000m,
            totalSessions: 4,
            visitId: DefaultVisitId);

        // Act
        package.RecordSession(
            "{\"energy\":15}", null, null, null,
            DefaultUserId, null, null, null, []);

        // Assert
        var evt = package.DomainEvents
            .OfType<TreatmentSessionCompletedEvent>()
            .First();

        // 4_000_000 / 4 = 1_000_000
        evt.SessionFeeAmount.Should().Be(1_000_000m);
    }

    [Fact]
    public void SessionCompletedEvent_NullVisitId_ShouldBeAllowed()
    {
        // Arrange
        var package = CreatePackage(visitId: null);

        // Act
        package.RecordSession(
            "{\"energy\":15}", null, null, null,
            DefaultUserId, null, null, null, []);

        // Assert
        var evt = package.DomainEvents
            .OfType<TreatmentSessionCompletedEvent>()
            .First();

        evt.VisitId.Should().BeNull();
    }

    // =========================================================================
    // PublishSessionCompletedIntegrationEventHandler maps VisitId and SessionFeeAmount
    // =========================================================================

    [Fact]
    public void PublishHandler_ShouldMapVisitIdAndSessionFeeAmountToIntegrationEvent()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var domainEvent = new TreatmentSessionCompletedEvent(
            PackageId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            PatientId: DefaultPatientId,
            TreatmentType: TreatmentType.IPL,
            Consumables: [],
            VisitId: visitId,
            SessionFeeAmount: 1_200_000m);

        // Act
        var integrationEvent = PublishSessionCompletedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.VisitId.Should().Be(visitId);
        integrationEvent.SessionFeeAmount.Should().Be(1_200_000m);
    }

    [Fact]
    public void PublishHandler_NullVisitId_ShouldMapCorrectly()
    {
        // Arrange
        var domainEvent = new TreatmentSessionCompletedEvent(
            PackageId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            PatientId: DefaultPatientId,
            TreatmentType: TreatmentType.IPL,
            Consumables: [],
            VisitId: null,
            SessionFeeAmount: 0m);

        // Act
        var integrationEvent = PublishSessionCompletedIntegrationEventHandler.Handle(domainEvent);

        // Assert
        integrationEvent.VisitId.Should().BeNull();
        integrationEvent.SessionFeeAmount.Should().Be(0m);
    }
}
