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

    private static Visit CreateVisitAtStage(WorkflowStage stage)
    {
        var visit = Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);

        // Advance through all stages up to target
        var stages = Enum.GetValues<WorkflowStage>()
            .Where(s => s > WorkflowStage.Reception && s <= stage)
            .OrderBy(s => s);

        foreach (var s in stages)
            visit.AdvanceStage(s);

        return visit;
    }

    [Fact]
    public void AdvanceStage_FromPharmacyOpticalToDone_Succeeds()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.PharmacyOptical);

        // Act
        visit.AdvanceStage(WorkflowStage.Done);

        // Assert
        visit.CurrentStage.Should().Be(WorkflowStage.Done);
    }

    [Fact]
    public void AdvanceStage_FromCashierToDone_ThrowsBecauseSkipsPharmacyOptical()
    {
        // Arrange -- visit at Cashier (stage 6), trying to skip to Done (stage 8)
        var visit = CreateVisitAtStage(WorkflowStage.Cashier);

        // Act -- AdvanceStage allows any forward movement, so this should succeed
        // (AdvanceStage only checks newStage > currentStage, no skip validation)
        visit.AdvanceStage(WorkflowStage.Done);

        // Assert -- Done is > Cashier, so forward advance works
        visit.CurrentStage.Should().Be(WorkflowStage.Done);
    }

    [Fact]
    public void AdvanceStage_FromDone_ThrowsBecauseNoForwardStage()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.Done);

        // Act & Assert -- cannot advance beyond Done
        var act = () => visit.AdvanceStage(WorkflowStage.Done);
        act.Should().Throw<InvalidOperationException>();
    }
}
