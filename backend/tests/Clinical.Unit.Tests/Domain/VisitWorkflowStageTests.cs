using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using Shared.Domain;

namespace Clinical.Unit.Tests.Domain;

/// <summary>
/// Tests for redesigned WorkflowStage enum and new domain enums/child entities.
/// Validates the 12-stage + Done=99 workflow, new enums, and child entity creation.
/// </summary>
public class VisitWorkflowStageTests
{
    // ===================== WorkflowStage Enum Tests =====================

    [Theory]
    [InlineData("Reception", 0)]
    [InlineData("RefractionVA", 1)]
    [InlineData("DoctorExam", 2)]
    [InlineData("Imaging", 3)]
    [InlineData("DoctorReviewsResults", 4)]
    [InlineData("Prescription", 5)]
    [InlineData("Cashier", 6)]
    [InlineData("Pharmacy", 7)]
    [InlineData("OpticalCenter", 8)]
    [InlineData("OpticalLab", 9)]
    [InlineData("ReturnGlasses", 10)]
    [InlineData("Done", 99)]
    public void WorkflowStage_HasExpectedValues(string name, int expectedValue)
    {
        var stage = Enum.Parse<WorkflowStage>(name);
        ((int)stage).Should().Be(expectedValue);
    }

    [Fact]
    public void WorkflowStage_HasExactly12Members()
    {
        var values = Enum.GetValues<WorkflowStage>();
        values.Should().HaveCount(12);
    }

    [Fact]
    public void WorkflowStage_DoesNotContainCashierGlasses()
    {
        var names = Enum.GetNames<WorkflowStage>();
        names.Should().NotContain("CashierGlasses");
    }

    [Fact]
    public void WorkflowStage_DoesNotContainOldNames()
    {
        var names = Enum.GetNames<WorkflowStage>();
        names.Should().NotContain("Diagnostics");
        names.Should().NotContain("DoctorReads");
        names.Should().NotContain("Rx");
        names.Should().NotContain("PharmacyOptical");
    }

    // ===================== SkipReason Enum Tests =====================

    [Theory]
    [InlineData("FollowUpExisting", 0)]
    [InlineData("PatientRefused", 1)]
    [InlineData("UnrelatedExam", 2)]
    [InlineData("Other", 3)]
    public void SkipReason_HasExpectedValues(string name, int expectedValue)
    {
        var reason = Enum.Parse<SkipReason>(name);
        ((int)reason).Should().Be(expectedValue);
    }

    // ===================== TrackStatus Enum Tests =====================

    [Theory]
    [InlineData("NotApplicable", 0)]
    [InlineData("Pending", 1)]
    [InlineData("InProgress", 2)]
    [InlineData("Completed", 3)]
    public void TrackStatus_HasExpectedValues(string name, int expectedValue)
    {
        var status = Enum.Parse<TrackStatus>(name);
        ((int)status).Should().Be(expectedValue);
    }

    // ===================== ImagingRequest Entity Tests =====================

    [Fact]
    public void ImagingRequest_CanBeCreatedWithDoctorIdAndNote()
    {
        var doctorId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var serviceNames = new List<string> { "OCT", "Fluorescein" };

        var request = ImagingRequest.Create(visitId, doctorId, "Check retina", serviceNames);

        request.DoctorId.Should().Be(doctorId);
        request.VisitId.Should().Be(visitId);
        request.DoctorNote.Should().Be("Check retina");
        request.Services.Should().HaveCount(2);
        request.RequestedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ===================== ImagingService Entity Tests =====================

    [Fact]
    public void ImagingService_HasNameEyeScopeAndIsCompleted()
    {
        var requestId = Guid.NewGuid();
        var service = ImagingService.Create(requestId, "OCT", "OD");

        service.ServiceName.Should().Be("OCT");
        service.EyeScope.Should().Be("OD");
        service.IsCompleted.Should().BeFalse();
        service.ImagingRequestId.Should().Be(requestId);
    }

    // ===================== StageSkip Entity Tests =====================

    [Fact]
    public void StageSkip_StoresReasonAndFreeTextNote()
    {
        var actorId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var skip = StageSkip.Create(visitId, WorkflowStage.RefractionVA,
            SkipReason.PatientRefused, "Patient refused refraction", actorId, "Dr. Test");

        skip.VisitId.Should().Be(visitId);
        skip.Stage.Should().Be(WorkflowStage.RefractionVA);
        skip.Reason.Should().Be(SkipReason.PatientRefused);
        skip.FreeTextNote.Should().Be("Patient refused refraction");
        skip.ActorId.Should().Be(actorId);
        skip.ActorName.Should().Be("Dr. Test");
        skip.SkippedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        skip.IsUndone.Should().BeFalse();
    }

    [Fact]
    public void StageSkip_FreeTextNote_MaxLength200()
    {
        var longNote = new string('x', 201);
        var act = () => StageSkip.Create(Guid.NewGuid(), WorkflowStage.RefractionVA,
            SkipReason.Other, longNote, Guid.NewGuid(), "Dr. Test");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*200*");
    }

    // ===================== VisitPayment Entity Tests =====================

    [Fact]
    public void VisitPayment_StoresAmountAndPaymentMethod()
    {
        var visitId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();
        var payment = VisitPayment.Create(visitId, PaymentType.Visit, 500000m,
            PaymentMethod.Cash, 600000m, 100000m, cashierId, "Cashier A");

        payment.VisitId.Should().Be(visitId);
        payment.PaymentKind.Should().Be(PaymentType.Visit);
        payment.Amount.Should().Be(500000m);
        payment.Method.Should().Be(PaymentMethod.Cash);
        payment.AmountReceived.Should().Be(600000m);
        payment.ChangeGiven.Should().Be(100000m);
        payment.CashierId.Should().Be(cashierId);
        payment.CashierName.Should().Be("Cashier A");
        payment.PaidAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ===================== PharmacyDispensing Entity Tests =====================

    [Fact]
    public void PharmacyDispensing_StoresMedicationLineItems()
    {
        var visitId = Guid.NewGuid();
        var pharmacistId = Guid.NewGuid();
        var lineItems = new List<(string DrugName, int Quantity, string Instruction)>
        {
            ("Amoxicillin", 20, "2 times daily"),
            ("Ibuprofen", 10, "As needed")
        };

        var dispensing = PharmacyDispensing.Create(visitId, pharmacistId, "Pharmacist B", lineItems);

        dispensing.VisitId.Should().Be(visitId);
        dispensing.PharmacistId.Should().Be(pharmacistId);
        dispensing.PharmacistName.Should().Be("Pharmacist B");
        dispensing.LineItems.Should().HaveCount(2);
        dispensing.LineItems.First().DrugName.Should().Be("Amoxicillin");
        dispensing.LineItems.First().IsDispensed.Should().BeFalse();
    }

    // ===================== OpticalOrder Entity Tests =====================

    [Fact]
    public void OpticalOrder_StoresLensTypeFrameCodeAndPricing()
    {
        var visitId = Guid.NewGuid();
        var consultantId = Guid.NewGuid();
        var order = OpticalOrder.Create(visitId, "Progressive", "FR-001",
            800000m, 500000m, 1300000m, consultantId, "Consultant C");

        order.VisitId.Should().Be(visitId);
        order.LensType.Should().Be("Progressive");
        order.FrameCode.Should().Be("FR-001");
        order.LensCostPerUnit.Should().Be(800000m);
        order.FrameCost.Should().Be(500000m);
        order.TotalPrice.Should().Be(1300000m);
        order.ConsultantId.Should().Be(consultantId);
        order.ConsultantName.Should().Be("Consultant C");
    }

    // ===================== HandoffChecklist Entity Tests =====================

    [Fact]
    public void HandoffChecklist_StoresVerificationFields()
    {
        var visitId = Guid.NewGuid();
        var completedById = Guid.NewGuid();
        var checklist = HandoffChecklist.Create(visitId, true, true, true,
            completedById, "Staff D");

        checklist.VisitId.Should().Be(visitId);
        checklist.PrescriptionVerified.Should().BeTrue();
        checklist.FrameCorrect.Should().BeTrue();
        checklist.PatientConfirmedFit.Should().BeTrue();
        checklist.CompletedById.Should().Be(completedById);
        checklist.CompletedByName.Should().Be("Staff D");
        checklist.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
