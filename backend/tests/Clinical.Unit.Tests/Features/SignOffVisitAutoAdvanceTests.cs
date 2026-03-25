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
        var visit = Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranchId, false);

        for (var s = WorkflowStage.RefractionVA; s <= stage; s++)
        {
            visit.AdvanceStage(s);
        }

        return visit;
    }

    [Fact]
    public async Task Handle_SignOffAtDoctorExam_AutoAdvancesToDiagnostics()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        var command = new SignOffVisitCommand(visit.Id);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.Diagnostics);
    }

    [Fact]
    public async Task Handle_SignOffAtDoctorReads_AutoAdvancesToRx()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorReads);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        var command = new SignOffVisitCommand(visit.Id);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.Rx);
    }

    [Fact]
    public async Task Handle_SignOffAtPharmacyOptical_DoesNotAdvance()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.PharmacyOptical);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        var command = new SignOffVisitCommand(visit.Id);

        // Act
        var result = await SignOffVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.PharmacyOptical);
    }
}
