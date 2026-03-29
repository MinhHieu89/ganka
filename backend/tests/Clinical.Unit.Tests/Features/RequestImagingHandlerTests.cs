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

public class RequestImagingHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public RequestImagingHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
    }

    private static Visit CreateVisitAtStage(WorkflowStage stage)
    {
        var visit = Visit.Create(Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);

        WorkflowStage[] path = [WorkflowStage.PreExam, WorkflowStage.DoctorExam,
            WorkflowStage.Prescription, WorkflowStage.Cashier];

        foreach (var s in path)
        {
            visit.AdvanceStage(s);
            if (s == stage) break;
        }

        return visit;
    }

    [Fact]
    public async Task Handle_AtDoctorExam_RequestsImagingAndAdvances()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        var services = new List<string> { "OCT", "Fluorescein" };
        var command = new RequestImagingCommand(visit.Id, "Check macula", services);
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await RequestImagingHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.ImagingRequested.Should().BeTrue();
        visit.CurrentStage.Should().Be(WorkflowStage.Imaging);
        visit.ImagingRequests.Should().HaveCount(1);
        visit.ImagingRequests.First().Services.Should().HaveCount(2);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotAtDoctorExam_ReturnsError()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.PreExam);
        var command = new RequestImagingCommand(visit.Id, null, new List<string> { "OCT" });
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await RequestImagingHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new RequestImagingCommand(Guid.NewGuid(), null, new List<string> { "OCT" });
        _visitRepository.GetByIdAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await RequestImagingHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_EmptyServices_ReturnsError()
    {
        // Arrange
        var visit = CreateVisitAtStage(WorkflowStage.DoctorExam);
        var command = new RequestImagingCommand(visit.Id, null, new List<string>());
        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await RequestImagingHandler.Handle(
            command, _visitRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
