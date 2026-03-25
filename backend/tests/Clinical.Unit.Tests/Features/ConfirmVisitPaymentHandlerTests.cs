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

public class ConfirmVisitPaymentHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public ConfirmVisitPaymentHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.Email.Returns("cashier@ganka28.com");
    }

    private static Visit CreateVisitAtCashier(bool hasDrugs = false, bool hasGlasses = false)
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);

        // Advance through no-imaging path to Cashier
        visit.AdvanceStage(WorkflowStage.RefractionVA);
        visit.AdvanceStage(WorkflowStage.DoctorExam);
        visit.AdvanceStage(WorkflowStage.Prescription);

        if (hasGlasses)
        {
            // Add optical prescription before going to OpticalCenter
            var opticalRx = OpticalPrescription.Create(visit.Id, LensType.SingleVision,
                null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null);
            visit.SetOpticalPrescription(opticalRx);
            visit.AdvanceStage(WorkflowStage.OpticalCenter);
            visit.AdvanceStage(WorkflowStage.Cashier);
        }
        else
        {
            visit.AdvanceStage(WorkflowStage.Cashier);
        }

        return visit;
    }

    [Fact]
    public async Task Handle_AtCashier_WithNeitherTrack_AdvancesToDone()
    {
        // Arrange
        var visit = CreateVisitAtCashier(hasDrugs: false, hasGlasses: false);
        var command = new ConfirmVisitPaymentCommand(visit.Id, 500_000, (int)PaymentMethod.Cash, 500_000, false);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await ConfirmVisitPaymentHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.VisitPayments.Should().HaveCount(1);
        visit.DrugTrackStatus.Should().Be(TrackStatus.NotApplicable);
        visit.GlassesTrackStatus.Should().Be(TrackStatus.NotApplicable);
        visit.CurrentStage.Should().Be(WorkflowStage.Done);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AtCashier_WithDrugsAndGlasses_ActivatesBothTracks()
    {
        // Arrange - visit with optical prescription (glasses path)
        var visit = CreateVisitAtCashier(hasGlasses: true);
        // Add a drug prescription
        var drugRx = DrugPrescription.Create(visit.Id, null);
        visit.AddDrugPrescription(drugRx);

        var command = new ConfirmVisitPaymentCommand(visit.Id, 2_000_000, (int)PaymentMethod.Card, 2_000_000, true);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await ConfirmVisitPaymentHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.DrugTrackStatus.Should().Be(TrackStatus.Pending);
        visit.GlassesTrackStatus.Should().Be(TrackStatus.Pending);
        visit.CurrentStage.Should().Be(WorkflowStage.Cashier); // stays at Cashier since tracks active
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new ConfirmVisitPaymentCommand(Guid.NewGuid(), 100, 0, 100, false);
        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await ConfirmVisitPaymentHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_NotAtCashier_ReturnsError()
    {
        // Arrange
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.RefractionVA);
        var command = new ConfirmVisitPaymentCommand(visit.Id, 500, 0, 500, false);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await ConfirmVisitPaymentHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PaymentCreatesRecord_WithCorrectChange()
    {
        // Arrange
        var visit = CreateVisitAtCashier();
        var command = new ConfirmVisitPaymentCommand(visit.Id, 500_000, (int)PaymentMethod.Cash, 1_000_000, false);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await ConfirmVisitPaymentHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var payment = visit.VisitPayments.First();
        payment.Amount.Should().Be(500_000);
        payment.AmountReceived.Should().Be(1_000_000);
        payment.ChangeGiven.Should().Be(500_000);
    }
}
