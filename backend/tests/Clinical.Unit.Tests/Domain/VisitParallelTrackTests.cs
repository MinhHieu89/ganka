using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using Shared.Domain;

namespace Clinical.Unit.Tests.Domain;

/// <summary>
/// Tests for Visit aggregate parallel track management, imaging loop, skip support,
/// and branching AdvanceStage logic.
/// </summary>
public class VisitParallelTrackTests
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    /// <summary>
    /// Creates a visit at the specified stage using the no-imaging path:
    /// Reception -> PreExam -> DoctorExam -> Prescription -> Cashier -> Pharmacy
    /// For Imaging/DoctorReviewsResults stages, uses the imaging path with RequestImaging.
    /// </summary>
    private static Visit CreateVisitAtStage(WorkflowStage stage)
    {
        if (stage is WorkflowStage.Imaging or WorkflowStage.DoctorReviewsResults)
            return CreateVisitAtStageWithImaging(stage);

        var visit = Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);

        // No-imaging path: DoctorExam skips directly to Prescription
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

    private static Visit CreateVisitAtStageWithImaging(WorkflowStage stage)
    {
        var visit = Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.PreExam);
        visit.AdvanceStage(WorkflowStage.DoctorExam);
        visit.RequestImaging(Guid.NewGuid(), null, new List<string> { "OCT" });
        visit.AdvanceStage(WorkflowStage.Imaging);
        if (stage == WorkflowStage.DoctorReviewsResults)
            visit.AdvanceStage(WorkflowStage.DoctorReviewsResults);
        return visit;
    }

    // ===================== Track Properties Default =====================

    [Fact]
    public void Visit_HasDrugTrackStatus_DefaultNotApplicable()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Test", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);
        visit.DrugTrackStatus.Should().Be(TrackStatus.NotApplicable);
    }

    [Fact]
    public void Visit_HasGlassesTrackStatus_DefaultNotApplicable()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Test", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);
        visit.GlassesTrackStatus.Should().Be(TrackStatus.NotApplicable);
    }

    [Fact]
    public void Visit_HasImagingRequested_DefaultFalse()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Test", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);
        visit.ImagingRequested.Should().BeFalse();
    }

    [Fact]
    public void Visit_HasRefractionSkipped_DefaultFalse()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Test", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);
        visit.RefractionSkipped.Should().BeFalse();
    }

    // ===================== RequestImaging =====================

    [Fact]
    public void RequestImaging_AtDoctorExam_SetsImagingRequestedTrue()
    {
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        var doctorId = Guid.NewGuid();

        visit.RequestImaging(doctorId, "Check retina", new List<string> { "OCT" });

        visit.ImagingRequested.Should().BeTrue();
        visit.ImagingRequests.Should().HaveCount(1);
    }

    [Fact]
    public void RequestImaging_NotAtDoctorExam_Throws()
    {
        var visit = CreateVisitAtStage(WorkflowStage.PreExam);

        var act = () => visit.RequestImaging(Guid.NewGuid(), "Note", new List<string> { "OCT" });

        act.Should().Throw<InvalidOperationException>();
    }

    // ===================== AdvanceStage Branching: Imaging =====================

    [Fact]
    public void AdvanceStage_DoctorExamToImaging_SucceedsIfImagingRequested()
    {
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        visit.RequestImaging(Guid.NewGuid(), null, new List<string> { "OCT" });

        visit.AdvanceStage(WorkflowStage.Imaging);

        visit.CurrentStage.Should().Be(WorkflowStage.Imaging);
    }

    [Fact]
    public void AdvanceStage_DoctorExamToPrescription_SucceedsIfImagingNotRequested()
    {
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        // No imaging requested

        visit.AdvanceStage(WorkflowStage.Prescription);

        visit.CurrentStage.Should().Be(WorkflowStage.Prescription);
    }

    [Fact]
    public void AdvanceStage_DoctorExamToPrescription_ThrowsIfImagingRequested()
    {
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        visit.RequestImaging(Guid.NewGuid(), null, new List<string> { "OCT" });

        var act = () => visit.AdvanceStage(WorkflowStage.Prescription);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*imaging*");
    }

    [Fact]
    public void AdvanceStage_DoctorReviewsResults_ToPrescription_Always()
    {
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        visit.RequestImaging(Guid.NewGuid(), null, new List<string> { "OCT" });
        visit.AdvanceStage(WorkflowStage.Imaging);
        visit.AdvanceStage(WorkflowStage.DoctorReviewsResults);

        visit.AdvanceStage(WorkflowStage.Prescription);

        visit.CurrentStage.Should().Be(WorkflowStage.Prescription);
    }

    // ===================== SkipRefraction =====================

    [Fact]
    public void SkipRefraction_SetsRefractionSkippedTrue_CreatesStageSkip()
    {
        var visit = CreateVisitAtStage(WorkflowStage.PreExam);
        var actorId = Guid.NewGuid();

        visit.SkipRefraction(SkipReason.FollowUpExisting, "Follow-up", actorId, "Dr. Test");

        visit.RefractionSkipped.Should().BeTrue();
        visit.StageSkips.Should().HaveCount(1);
        visit.StageSkips.First().Reason.Should().Be(SkipReason.FollowUpExisting);
    }

    [Fact]
    public void UndoRefractionSkip_SetsRefractionSkippedFalse()
    {
        var visit = CreateVisitAtStage(WorkflowStage.PreExam);
        visit.SkipRefraction(SkipReason.FollowUpExisting, null, Guid.NewGuid(), "Dr. Test");

        visit.UndoRefractionSkip();

        visit.RefractionSkipped.Should().BeFalse();
        visit.StageSkips.First().IsUndone.Should().BeTrue();
    }

    [Fact]
    public void AdvanceStage_FromPreExam_ToDoctorExam_SucceedsEvenIfSkipped()
    {
        var visit = CreateVisitAtStage(WorkflowStage.PreExam);
        visit.SkipRefraction(SkipReason.PatientRefused, null, Guid.NewGuid(), "Dr. Test");

        visit.AdvanceStage(WorkflowStage.DoctorExam);

        visit.CurrentStage.Should().Be(WorkflowStage.DoctorExam);
    }

    // ===================== ActivatePostPaymentTracks =====================

    [Fact]
    public void ActivatePostPaymentTracks_HasDrugs_DrugTrackPending()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);

        visit.ActivatePostPaymentTracks(hasDrugs: true, hasGlasses: false);

        visit.DrugTrackStatus.Should().Be(TrackStatus.Pending);
        visit.GlassesTrackStatus.Should().Be(TrackStatus.NotApplicable);
    }

    [Fact]
    public void ActivatePostPaymentTracks_HasGlasses_GlassesTrackPending()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);

        visit.ActivatePostPaymentTracks(hasDrugs: false, hasGlasses: true);

        visit.DrugTrackStatus.Should().Be(TrackStatus.NotApplicable);
        visit.GlassesTrackStatus.Should().Be(TrackStatus.Pending);
    }

    [Fact]
    public void ActivatePostPaymentTracks_Neither_BothNotApplicable()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);

        visit.ActivatePostPaymentTracks(hasDrugs: false, hasGlasses: false);

        visit.DrugTrackStatus.Should().Be(TrackStatus.NotApplicable);
        visit.GlassesTrackStatus.Should().Be(TrackStatus.NotApplicable);
    }

    // ===================== CompleteDrugTrack / CompleteGlassesTrack =====================

    [Fact]
    public void CompleteDrugTrack_SetsDrugTrackCompleted()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);
        visit.ActivatePostPaymentTracks(hasDrugs: true, hasGlasses: false);

        visit.CompleteDrugTrack();

        visit.DrugTrackStatus.Should().Be(TrackStatus.Completed);
    }

    [Fact]
    public void CompleteGlassesTrack_SetsGlassesTrackCompleted()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);
        visit.ActivatePostPaymentTracks(hasDrugs: false, hasGlasses: true);

        visit.CompleteGlassesTrack();

        visit.GlassesTrackStatus.Should().Be(TrackStatus.Completed);
    }

    // ===================== IsComplete =====================

    [Fact]
    public void IsComplete_BothTracksCompleted_ReturnsTrue()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);
        visit.ActivatePostPaymentTracks(hasDrugs: true, hasGlasses: true);
        visit.CompleteDrugTrack();
        visit.CompleteGlassesTrack();

        visit.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_OnlyDrugTrackActive_CompletedReturnsTrue()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);
        visit.ActivatePostPaymentTracks(hasDrugs: true, hasGlasses: false);
        visit.CompleteDrugTrack();

        visit.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_DrugTrackPending_ReturnsFalse()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);
        visit.ActivatePostPaymentTracks(hasDrugs: true, hasGlasses: false);

        visit.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_NeitherActivated_ReturnsTrue()
    {
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);
        visit.ActivatePostPaymentTracks(hasDrugs: false, hasGlasses: false);

        visit.IsComplete.Should().BeTrue();
    }

    // ===================== Collections =====================

    [Fact]
    public void Visit_HasCollections_ForAllChildEntities()
    {
        var visit = Visit.Create(Guid.NewGuid(), "Test", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);

        visit.ImagingRequests.Should().NotBeNull().And.BeEmpty();
        visit.StageSkips.Should().NotBeNull().And.BeEmpty();
        visit.VisitPayments.Should().NotBeNull().And.BeEmpty();
        visit.PharmacyDispensings.Should().NotBeNull().And.BeEmpty();
        visit.OpticalOrders.Should().NotBeNull().And.BeEmpty();
        visit.HandoffChecklists.Should().NotBeNull().And.BeEmpty();
    }

    // ===================== ActiveVisitDto Fields =====================

    [Fact]
    public void ActiveVisitDto_HasNewTrackFields()
    {
        // Verify ActiveVisitDto has the new fields by constructing one
        var dto = new Clinical.Contracts.Dtos.ActiveVisitDto(
            Guid.NewGuid(), Guid.NewGuid(), "Test", "Dr. Test",
            0, DateTime.UtcNow, false, 5, false,
            (int)TrackStatus.NotApplicable, (int)TrackStatus.NotApplicable,
            false, false);

        dto.DrugTrackStatus.Should().Be((int)TrackStatus.NotApplicable);
        dto.GlassesTrackStatus.Should().Be((int)TrackStatus.NotApplicable);
        dto.ImagingRequested.Should().BeFalse();
        dto.RefractionSkipped.Should().BeFalse();
    }
}
