using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using Shared.Domain;

namespace Clinical.Unit.Tests.Domain;

/// <summary>
/// Tests for Visit.ReverseStage domain method.
/// Validates allowed reversal transitions per D-07 and reason requirement per D-09.
/// </summary>
public class VisitReverseStageTests
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Visit CreateVisitAtStage(WorkflowStage stage)
    {
        var visit = Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);

        // Advance to the desired stage step by step
        for (var s = WorkflowStage.RefractionVA; s <= stage; s++)
        {
            visit.AdvanceStage(s);
        }

        return visit;
    }

    [Fact]
    public void ReverseStage_FromRefractionVA_ToReception_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.RefractionVA);

        // Act
        visit.ReverseStage(WorkflowStage.Reception, "Wrong patient");

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.Reception);
    }

    [Fact]
    public void ReverseStage_FromDoctorExam_ToRefractionVA_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);

        // Act
        visit.ReverseStage(WorkflowStage.RefractionVA, "Need re-refraction");

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.RefractionVA);
    }

    [Fact]
    public void ReverseStage_FromDoctorReads_ToDoctorExam_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorReads);

        // Act
        visit.ReverseStage(WorkflowStage.DoctorExam, "Need additional exam");

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.DoctorExam);
    }

    [Fact]
    public void ReverseStage_FromRx_ToDoctorReads_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.Rx);

        // Act
        visit.ReverseStage(WorkflowStage.DoctorReads, "Need re-read");

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.DoctorReads);
    }

    [Fact]
    public void ReverseStage_FromCashier_ToAnyStage_ThrowsNotAllowed()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);

        // Act & Assert
        var act = () => visit.ReverseStage(WorkflowStage.DoctorExam, "Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not allowed*");
    }

    [Fact]
    public void ReverseStage_FromPharmacyOptical_ToAnyStage_ThrowsNotAllowed()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.PharmacyOptical);

        // Act & Assert
        var act = () => visit.ReverseStage(WorkflowStage.Rx, "Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not allowed*");
    }

    [Fact]
    public void ReverseStage_FromDoctorExam_ToReception_ThrowsNotInAllowedTable()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);

        // Act & Assert -- DoctorExam can only go to RefractionVA, not Reception
        var act = () => visit.ReverseStage(WorkflowStage.Reception, "Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not allowed*");
    }

    [Fact]
    public void ReverseStage_WithEmptyReason_ThrowsReasonRequired()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.RefractionVA);

        // Act & Assert
        var act = () => visit.ReverseStage(WorkflowStage.Reception, "");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Reason is required*");
    }

    [Fact]
    public void ReverseStage_WithNullReason_ThrowsReasonRequired()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.RefractionVA);

        // Act & Assert
        var act = () => visit.ReverseStage(WorkflowStage.Reception, null!);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Reason is required*");
    }

    [Fact]
    public void ReverseStage_ToSameStage_ThrowsInvalidOperation()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);

        // Act & Assert
        var act = () => visit.ReverseStage(WorkflowStage.DoctorExam, "Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*earlier*");
    }

    [Fact]
    public void ReverseStage_ToHigherStage_ThrowsInvalidOperation()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);

        // Act & Assert
        var act = () => visit.ReverseStage(WorkflowStage.Diagnostics, "Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*earlier*");
    }
}
