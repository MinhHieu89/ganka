using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class SetPrimaryDiagnosisHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Visit CreateEditableVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
    }

    [Fact]
    public async Task Handle_WithValidDiagnosisId_SetsPrimaryAndDemotesOther()
    {
        // Arrange
        var visit = CreateEditableVisit();
        var primaryDiag = VisitDiagnosis.Create(
            visit.Id, "H04.121", "Dry Eye", "Kho mat",
            Laterality.OD, DiagnosisRole.Primary, 0);
        visit.AddDiagnosis(primaryDiag);

        var secondaryDiag = VisitDiagnosis.Create(
            visit.Id, "H40.11", "Glaucoma", "Glaucom",
            Laterality.OD, DiagnosisRole.Secondary, 1);
        visit.AddDiagnosis(secondaryDiag);

        var command = new SetPrimaryDiagnosisCommand(visit.Id, secondaryDiag.Id);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SetPrimaryDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        secondaryDiag.Role.Should().Be(DiagnosisRole.Primary);
        primaryDiag.Role.Should().Be(DiagnosisRole.Secondary);
    }

    [Fact]
    public async Task Handle_WithSignedVisit_ReturnsFailure()
    {
        // Arrange
        var visit = CreateEditableVisit();
        var primaryDiag = VisitDiagnosis.Create(
            visit.Id, "H04.121", "Dry Eye", "Kho mat",
            Laterality.OD, DiagnosisRole.Primary, 0);
        visit.AddDiagnosis(primaryDiag);

        var secondaryDiag = VisitDiagnosis.Create(
            visit.Id, "H40.11", "Glaucoma", "Glaucom",
            Laterality.OD, DiagnosisRole.Secondary, 1);
        visit.AddDiagnosis(secondaryDiag);

        visit.SignOff(Guid.NewGuid());

        var command = new SetPrimaryDiagnosisCommand(visit.Id, secondaryDiag.Id);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SetPrimaryDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_WithNonExistentVisitId_ReturnsFailure()
    {
        // Arrange
        var command = new SetPrimaryDiagnosisCommand(Guid.NewGuid(), Guid.NewGuid());
        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await SetPrimaryDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_AlreadyPrimary_ReturnsSuccess()
    {
        // Arrange
        var visit = CreateEditableVisit();
        var primaryDiag = VisitDiagnosis.Create(
            visit.Id, "H04.121", "Dry Eye", "Kho mat",
            Laterality.OD, DiagnosisRole.Primary, 0);
        visit.AddDiagnosis(primaryDiag);

        var command = new SetPrimaryDiagnosisCommand(visit.Id, primaryDiag.Id);
        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await SetPrimaryDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        primaryDiag.Role.Should().Be(DiagnosisRole.Primary);
    }
}
