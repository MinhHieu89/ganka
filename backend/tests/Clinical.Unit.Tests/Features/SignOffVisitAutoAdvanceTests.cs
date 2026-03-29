using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

/// <summary>
/// Tests for the auto-advance behavior after sign-off (D-11).
/// Sign-off at a doctor stage should automatically advance to the next stage.
/// </summary>
public class SignOffVisitAutoAdvanceTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DoctorId = Guid.NewGuid();
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public SignOffVisitAutoAdvanceTests()
    {
        _currentUser.UserId.Returns(DoctorId);
    }

    private static Visit CreateVisitAtStage(WorkflowStage stage)
    {
        if (stage is WorkflowStage.DoctorReviewsResults)
        {
            var imgVisit = Visit.Create(Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
                DefaultBranchId, false);
            imgVisit.AdvanceStage(WorkflowStage.PreExam);
            imgVisit.AdvanceStage(WorkflowStage.DoctorExam);
            imgVisit.RequestImaging(Guid.NewGuid(), null, new List<string> { "OCT" });
            imgVisit.AdvanceStage(WorkflowStage.Imaging);
            imgVisit.AdvanceStage(WorkflowStage.DoctorReviewsResults);
            return imgVisit;
        }

        var visit = Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);

        // No-imaging path
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
    public async Task Handle_SignOffAtDoctorExam_NoImaging_AutoAdvancesToPrescription()
    {
        // Arrange - no imaging requested, so auto-advance goes to Prescription
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        var command = new SignOffVisitCommand(visit.Id);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.Prescription);
    }

    [Fact]
    public async Task Handle_SignOffAtDoctorReviewsResults_AutoAdvancesToPrescription()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorReviewsResults);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        var command = new SignOffVisitCommand(visit.Id);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.Prescription);
    }

    [Fact]
    public async Task Handle_SignOffAtPharmacy_DoesNotAdvance()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.Pharmacy);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        var command = new SignOffVisitCommand(visit.Id);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.Pharmacy);
    }
}
