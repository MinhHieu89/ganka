using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class CancelVisitWithReasonTests
{
    private readonly IVisitRepository _visitRepo = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CancelVisitWithReasonCommand> _validator = Substitute.For<IValidator<CancelVisitWithReasonCommand>>();

    private static readonly BranchId DefaultBranch = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<CancelVisitWithReasonCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private Visit CreateDraftVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Test Patient", Guid.NewGuid(), "Dr. Test",
            DefaultBranch, false);
    }

    [Fact]
    public async Task Handle_DraftVisit_CancelsWithReason()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateDraftVisit();
        var userId = Guid.NewGuid();
        var command = new CancelVisitWithReasonCommand(visit.Id, "Patient decided not to continue", userId);

        _visitRepo.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        // Act
        var result = await CancelVisitWithReasonHandler.Handle(
            command, _visitRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Status.Should().Be(VisitStatus.Cancelled);
        visit.CancelledReason.Should().Be("Patient decided not to continue");
        visit.CancelledBy.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_SignedVisit_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateDraftVisit();
        visit.SignOff(Guid.NewGuid()); // Make it signed

        var command = new CancelVisitWithReasonCommand(visit.Id, "test reason", Guid.NewGuid());

        _visitRepo.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>())
            .Returns(visit);

        // Act
        var result = await CancelVisitWithReasonHandler.Handle(
            command, _visitRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_NotFoundVisit_ReturnsNotFound()
    {
        // Arrange
        SetupValidValidator();
        var command = new CancelVisitWithReasonCommand(Guid.NewGuid(), "reason", Guid.NewGuid());

        _visitRepo.GetByIdAsync(command.VisitId, Arg.Any<CancellationToken>())
            .Returns((Visit?)null);

        // Act
        var result = await CancelVisitWithReasonHandler.Handle(
            command, _visitRepo, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
