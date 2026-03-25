using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using Shared.Domain;

namespace Clinical.Unit.Tests.Domain;

/// <summary>
/// Tests for Visit domain entity, specifically AdvanceStage to Done.
/// </summary>
public class VisitTests
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Visit CreateVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);
    }

    [Fact]
    public void AdvanceStage_FromPharmacyToDone_Succeeds()
    {
        // Arrange — advance through no-imaging drug-only path
        var visit = CreateVisit();
        visit.AdvanceStage(WorkflowStage.RefractionVA);
        visit.AdvanceStage(WorkflowStage.DoctorExam);
        visit.AdvanceStage(WorkflowStage.Prescription);
        visit.AdvanceStage(WorkflowStage.Cashier);
        visit.AdvanceStage(WorkflowStage.Pharmacy);

        // Act
        visit.AdvanceStage(WorkflowStage.Done);

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.Done);
    }

    [Fact]
    public void AdvanceStage_FromCashierToDone_Succeeds()
    {
        // Arrange — visit at Cashier, advance directly to Done
        var visit = CreateVisit();
        visit.AdvanceStage(WorkflowStage.RefractionVA);
        visit.AdvanceStage(WorkflowStage.DoctorExam);
        visit.AdvanceStage(WorkflowStage.Prescription);
        visit.AdvanceStage(WorkflowStage.Cashier);

        // Act
        visit.AdvanceStage(WorkflowStage.Done);

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.Done);
    }

    [Fact]
    public void AdvanceStage_FromDone_ThrowsBecauseNoForwardStage()
    {
        // Arrange
        var visit = CreateVisit();
        visit.AdvanceStage(WorkflowStage.RefractionVA);
        visit.AdvanceStage(WorkflowStage.DoctorExam);
        visit.AdvanceStage(WorkflowStage.Prescription);
        visit.AdvanceStage(WorkflowStage.Cashier);
        visit.AdvanceStage(WorkflowStage.Done);

        // Act & Assert -- cannot advance beyond Done
        var act = () => visit.AdvanceStage(WorkflowStage.Done);
        act.Should().Throw<InvalidOperationException>();
    }
}
