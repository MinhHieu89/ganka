using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class AmendVisitHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<AmendVisitCommand> _validator = Substitute.For<IValidator<AmendVisitCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DoctorId = Guid.NewGuid();
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public AmendVisitHandlerTests()
    {
        _currentUser.UserId.Returns(DoctorId);
        _currentUser.Email.Returns("doctor@test.com");
    }

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<AmendVisitCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static Visit CreateSignedVisit()
    {
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        visit.SignOff(Guid.NewGuid());
        return visit;
    }

    [Fact]
    public async Task Handle_SignedVisitWithReason_CreatesAmendmentAndChangesStatus()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateSignedVisit();
        var command = new AmendVisitCommand(
            visit.Id,
            "Corrected diagnosis laterality",
            "[{\"FieldName\":\"Laterality\",\"OldValue\":\"OD\",\"NewValue\":\"OS\"}]");

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AmendVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Status.Should().Be(VisitStatus.Amended);
        visit.Amendments.Should().HaveCount(1);
        visit.Amendments.First().Reason.Should().Be("Corrected diagnosis laterality");
    }

    [Fact]
    public async Task Handle_EmptyReason_ReturnsValidationError()
    {
        // Arrange
        var command = new AmendVisitCommand(Guid.NewGuid(), "", "[]");

        _validator.ValidateAsync(Arg.Any<AmendVisitCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Reason", "Amendment reason is required.")
            }));

        // Act
        var result = await AmendVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_DraftVisit_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        // visit is Draft, not Signed

        var command = new AmendVisitCommand(visit.Id, "Some reason", "[{}]");
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AmendVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidValidator();
        var command = new AmendVisitCommand(Guid.NewGuid(), "Reason", "[{}]");
        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await AmendVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_ValidAmendment_SaveChangesCalled()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateSignedVisit();
        var command = new AmendVisitCommand(visit.Id, "Fix typo", "[{}]");
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        await AmendVisitHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
