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

    /// <summary>
    /// Advances a visit through the no-imaging path to reach the desired stage.
    /// For Imaging/DoctorReviewsResults stages, uses the imaging path.
    /// </summary>
    private static Visit CreateVisitAtStage(WorkflowStage stage)
    {
        if (stage is WorkflowStage.Imaging or WorkflowStage.DoctorReviewsResults)
        {
            var imgVisit = Visit.Create(Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
                DefaultBranchId, false);
            imgVisit.AdvanceStage(WorkflowStage.PreExam);
            imgVisit.AdvanceStage(WorkflowStage.DoctorExam);
            imgVisit.RequestImaging(Guid.NewGuid(), null, new List<string> { "OCT" });
            imgVisit.AdvanceStage(WorkflowStage.Imaging);
            if (stage == WorkflowStage.DoctorReviewsResults)
                imgVisit.AdvanceStage(WorkflowStage.DoctorReviewsResults);
            return imgVisit;
        }

        var visit = Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);

        // No-imaging path: DoctorExam -> Prescription (skips Imaging/DoctorReviewsResults)
        WorkflowStage[] noImagingPath =
        [
            WorkflowStage.PreExam, WorkflowStage.DoctorExam,
            WorkflowStage.Prescription, WorkflowStage.Cashier, WorkflowStage.Pharmacy
        ];

        foreach (var s in noImagingPath)
        {
            visit.AdvanceStage(s);
            if (s == stage) break;
        }

        return visit;
    }

    [Fact]
    public void ReverseStage_FromPreExam_ToReception_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.PreExam);

        // Act
        visit.ReverseStage(WorkflowStage.Reception, "Wrong patient");

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.Reception);
    }

    [Fact]
    public void ReverseStage_FromDoctorExam_ToPreExam_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);

        // Act
        visit.ReverseStage(WorkflowStage.PreExam, "Need re-refraction");

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.PreExam);
    }

    [Fact]
    public void ReverseStage_FromDoctorReviewsResults_ToDoctorExam_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorReviewsResults);

        // Act
        visit.ReverseStage(WorkflowStage.DoctorExam, "Need additional exam");

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.DoctorExam);
    }

    [Fact]
    public void ReverseStage_FromPrescription_ToDoctorReviewsResults_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.Prescription);

        // Act
        visit.ReverseStage(WorkflowStage.DoctorReviewsResults, "Need re-read");

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.DoctorReviewsResults);
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
    public void ReverseStage_FromPharmacy_ToAnyStage_ThrowsNotAllowed()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.Pharmacy);

        // Act & Assert
        var act = () => visit.ReverseStage(WorkflowStage.Prescription, "Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not allowed*");
    }

    [Fact]
    public void ReverseStage_FromDoctorExam_ToReception_ThrowsNotInAllowedTable()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);

        // Act & Assert -- DoctorExam can only go to PreExam, not Reception
        var act = () => visit.ReverseStage(WorkflowStage.Reception, "Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not allowed*");
    }

    [Fact]
    public void ReverseStage_WithEmptyReason_ThrowsReasonRequired()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.PreExam);

        // Act & Assert
        var act = () => visit.ReverseStage(WorkflowStage.Reception, "");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Reason is required*");
    }

    [Fact]
    public void ReverseStage_WithNullReason_ThrowsReasonRequired()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.PreExam);

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
        var act = () => visit.ReverseStage(WorkflowStage.Imaging, "Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*earlier*");
    }
}
