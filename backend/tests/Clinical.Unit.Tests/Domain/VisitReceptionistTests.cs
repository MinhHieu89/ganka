using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using Shared.Domain;

namespace Clinical.Unit.Tests.Domain;

/// <summary>
/// Tests for receptionist-related Visit domain extensions:
/// CancelWithReason, VisitSource, and Reason field.
/// </summary>
public class VisitReceptionistTests
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Visit CreateVisit(VisitSource source = VisitSource.Appointment, string? reason = null)
    {
        return Visit.Create(
            Guid.NewGuid(), "Test Patient",
            Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false,
            source: source,
            reason: reason);
    }

    // ==================== CancelWithReason Tests ====================

    [Fact]
    public void CancelWithReason_DraftVisit_SetsCancelledReasonAndBy()
    {
        var visit = CreateVisit();
        var userId = Guid.NewGuid();

        visit.CancelWithReason("BN bo ve", userId);

        visit.Status.Should().Be(VisitStatus.Cancelled);
        visit.CancelledReason.Should().Be("BN bo ve");
        visit.CancelledBy.Should().Be(userId);
    }

    [Fact]
    public void CancelWithReason_NonDraftVisit_Throws()
    {
        var visit = CreateVisit();
        visit.SignOff(Guid.NewGuid());

        var act = () => visit.CancelWithReason("reason", Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_StillWorks_BackwardCompat()
    {
        var visit = CreateVisit();

        visit.Cancel();

        visit.Status.Should().Be(VisitStatus.Cancelled);
    }

    // ==================== VisitSource Tests ====================

    [Fact]
    public void Create_WithWalkInSource_SetsSource()
    {
        var visit = CreateVisit(VisitSource.WalkIn);

        visit.Source.Should().Be(VisitSource.WalkIn);
    }

    [Fact]
    public void Create_DefaultSource_IsAppointment()
    {
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient",
            Guid.NewGuid(), "Doctor",
            DefaultBranchId, false);

        visit.Source.Should().Be(VisitSource.Appointment);
    }

    // ==================== Reason Tests ====================

    [Fact]
    public void Create_WithReason_SetsReason()
    {
        var visit = CreateVisit(reason: "Kham mat");

        visit.Reason.Should().Be("Kham mat");
    }
}
