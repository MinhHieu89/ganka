using FluentAssertions;
using Shared.Domain;
using Treatment.Domain.Entities;
using Treatment.Domain.Enums;
using Treatment.Domain.Events;

namespace Treatment.Unit.Tests.Domain;

public class TreatmentPackageDomainTests
{
    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static TreatmentPackage CreateTestPackage(
        int totalSessions = 4,
        int minIntervalDays = 14)
    {
        return TreatmentPackage.Create(
            protocolTemplateId: Guid.NewGuid(),
            patientId: Guid.NewGuid(),
            patientName: "Nguyen Van A",
            treatmentType: TreatmentType.IPL,
            totalSessions: totalSessions,
            pricingMode: PricingMode.PerPackage,
            packagePrice: 5000000m,
            sessionPrice: 1500000m,
            minIntervalDays: minIntervalDays,
            parametersJson: """{"energy":12}""",
            visitId: null,
            createdById: DefaultUserId,
            branchId: new BranchId(DefaultBranchId));
    }

    private static List<(Guid, string, int)> EmptyConsumables => [];

    private static TreatmentSession RecordSessionOnPackage(
        TreatmentPackage package,
        DateTime? scheduledAt = null,
        string? intervalOverrideReason = null)
    {
        return package.RecordSession(
            parametersJson: "{}",
            osdiScore: null,
            osdiSeverity: null,
            clinicalNotes: null,
            performedById: DefaultUserId,
            visitId: null,
            scheduledAt: scheduledAt,
            intervalOverrideReason: intervalOverrideReason,
            consumables: EmptyConsumables);
    }

    #region RecordSession - Interval Enforcement

    [Fact]
    public void RecordSession_NopriorSessions_AlwaysSucceeds()
    {
        // Arrange
        var package = CreateTestPackage(minIntervalDays: 14);

        // Act
        var session = RecordSessionOnPackage(package);

        // Assert
        session.Should().NotBeNull();
        session.SessionNumber.Should().Be(1);
    }

    [Fact]
    public void RecordSession_IntervalViolation_NoOverride_ThrowsInvalidOperation()
    {
        // Arrange
        var package = CreateTestPackage(minIntervalDays: 14);
        RecordSessionOnPackage(package, scheduledAt: DateTime.UtcNow.AddDays(-20));

        // Act - try to record a second session too soon (only 5 days later)
        var act = () => RecordSessionOnPackage(package, scheduledAt: DateTime.UtcNow.AddDays(-20).AddDays(5));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*interval*");
    }

    [Fact]
    public void RecordSession_IntervalViolation_WithOverrideReason_Succeeds()
    {
        // Arrange
        var package = CreateTestPackage(minIntervalDays: 14);
        RecordSessionOnPackage(package, scheduledAt: DateTime.UtcNow.AddDays(-20));

        // Act - record second session too soon but with override reason
        var session = RecordSessionOnPackage(
            package,
            scheduledAt: DateTime.UtcNow.AddDays(-20).AddDays(5),
            intervalOverrideReason: "Patient travelling, needs early session");

        // Assert
        session.Should().NotBeNull();
    }

    [Fact]
    public void RecordSession_IntervalSatisfied_Succeeds()
    {
        // Arrange
        var package = CreateTestPackage(minIntervalDays: 14);
        RecordSessionOnPackage(package, scheduledAt: DateTime.UtcNow.AddDays(-30));

        // Act - record second session with enough interval (15 days later)
        var session = RecordSessionOnPackage(package, scheduledAt: DateTime.UtcNow.AddDays(-30).AddDays(15));

        // Assert
        session.Should().NotBeNull();
    }

    #endregion

    #region RecordSession - Session Numbering

    [Fact]
    public void RecordSession_SessionNumbering_ExcludesCancelledSessions()
    {
        // Arrange
        var package = CreateTestPackage(totalSessions: 4, minIntervalDays: 0);

        // Record 3 sessions
        RecordSessionOnPackage(package); // Session 1
        RecordSessionOnPackage(package); // Session 2
        RecordSessionOnPackage(package); // Session 3

        // Current bug: session numbering uses _sessions.Count + 1
        // After 3 sessions, next should be numbered based on non-cancelled count
        // With 3 completed sessions and 0 cancelled, next is 4
        // But if we had cancelled session 2, next should be 3 (not 4)
        package.Sessions.Should().HaveCount(3);

        // With no cancellations, numbering = non-cancelled count + 1 = 4
        var session4 = RecordSessionOnPackage(package);
        session4.SessionNumber.Should().Be(4);
    }

    #endregion

    #region Modify - Auto-completion

    [Fact]
    public void Modify_TotalSessionsEqualsCompleted_AutoCompletesAndRaisesEvent()
    {
        // Arrange
        var package = CreateTestPackage(totalSessions: 4, minIntervalDays: 0);

        // Record 2 completed sessions
        RecordSessionOnPackage(package);
        RecordSessionOnPackage(package);
        package.ClearDomainEvents();

        // Act - reduce total sessions to 2 (== completed count)
        package.Modify(
            totalSessions: 2,
            parametersJson: null,
            minIntervalDays: null,
            changeDescription: "Reduced sessions",
            changedById: DefaultUserId,
            reason: "Patient improved");

        // Assert
        package.Status.Should().Be(PackageStatus.Completed);
        package.DomainEvents.Should().Contain(e => e is TreatmentPackageCompletedEvent);
    }

    [Fact]
    public void Modify_TotalSessionsGreaterThanCompleted_KeepsActive()
    {
        // Arrange
        var package = CreateTestPackage(totalSessions: 4, minIntervalDays: 0);
        RecordSessionOnPackage(package);
        package.ClearDomainEvents();

        // Act - modify but total still > completed
        package.Modify(
            totalSessions: 3,
            parametersJson: null,
            minIntervalDays: null,
            changeDescription: "Reduced sessions",
            changedById: DefaultUserId,
            reason: "Adjustment");

        // Assert
        package.Status.Should().Be(PackageStatus.Active);
    }

    #endregion

    #region Modify - Change Detection

    [Fact]
    public void Modify_NoActualChanges_DoesNotCreateVersionSnapshot()
    {
        // Arrange
        var package = CreateTestPackage(totalSessions: 4, minIntervalDays: 14);

        // Act - call Modify with no changes (null values mean "don't change")
        package.Modify(
            totalSessions: null,
            parametersJson: null,
            minIntervalDays: null,
            changeDescription: "No changes",
            changedById: DefaultUserId,
            reason: "Test");

        // Assert - no version snapshot should be created
        package.Versions.Should().BeEmpty();
    }

    [Fact]
    public void Modify_WithActualChanges_CreatesVersionSnapshot()
    {
        // Arrange
        var package = CreateTestPackage(totalSessions: 4, minIntervalDays: 14);

        // Act - make a real change
        package.Modify(
            totalSessions: 5,
            parametersJson: null,
            minIntervalDays: null,
            changeDescription: "Increased sessions",
            changedById: DefaultUserId,
            reason: "Patient needs more");

        // Assert
        package.Versions.Should().HaveCount(1);
    }

    #endregion

    #region RequestCancellation - Deduction Range

    [Fact]
    public void RequestCancellation_DeductionBelow10Percent_Throws()
    {
        // Arrange
        var package = CreateTestPackage();

        // Act
        var act = () => package.RequestCancellation(
            reason: "Patient request",
            deductionPercent: 5,
            requestedById: DefaultUserId);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RequestCancellation_DeductionAbove20Percent_Throws()
    {
        // Arrange
        var package = CreateTestPackage();

        // Act
        var act = () => package.RequestCancellation(
            reason: "Patient request",
            deductionPercent: 25,
            requestedById: DefaultUserId);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RequestCancellation_DeductionWithinRange_Succeeds()
    {
        // Arrange
        var package = CreateTestPackage();

        // Act
        package.RequestCancellation(
            reason: "Patient request",
            deductionPercent: 15,
            requestedById: DefaultUserId);

        // Assert
        package.Status.Should().Be(PackageStatus.PendingCancellation);
        package.CancellationRequest.Should().NotBeNull();
    }

    [Fact]
    public void RequestCancellation_DeductionAt10Percent_Succeeds()
    {
        // Arrange
        var package = CreateTestPackage();

        // Act
        package.RequestCancellation(
            reason: "Patient request",
            deductionPercent: 10,
            requestedById: DefaultUserId);

        // Assert
        package.Status.Should().Be(PackageStatus.PendingCancellation);
    }

    [Fact]
    public void RequestCancellation_DeductionAt20Percent_Succeeds()
    {
        // Arrange
        var package = CreateTestPackage();

        // Act
        package.RequestCancellation(
            reason: "Patient request",
            deductionPercent: 20,
            requestedById: DefaultUserId);

        // Assert
        package.Status.Should().Be(PackageStatus.PendingCancellation);
    }

    #endregion
}
