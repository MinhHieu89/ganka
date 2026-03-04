using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class AddVisitDiagnosisHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<AddVisitDiagnosisCommand> _validator = Substitute.For<IValidator<AddVisitDiagnosisCommand>>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<AddVisitDiagnosisCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static Visit CreateEditableVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
    }

    [Fact]
    public async Task Handle_OdLaterality_StoresDiagnosis()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        var command = new AddVisitDiagnosisCommand(
            visit.Id, "H04.121", "Dry Eye, right", "Kho mat, phai",
            (int)Laterality.OD, (int)DiagnosisRole.Primary, 1);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddVisitDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Diagnoses.Should().HaveCount(1);
        visit.Diagnoses.First().Laterality.Should().Be(Laterality.OD);
        visit.Diagnoses.First().Icd10Code.Should().Be("H04.121");
    }

    [Fact]
    public async Task Handle_OuLaterality_CreatesTwoRecords()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        var command = new AddVisitDiagnosisCommand(
            visit.Id, "H04.12", "Dry Eye", "Kho mat",
            (int)Laterality.OU, (int)DiagnosisRole.Primary, 1);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddVisitDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Diagnoses.Should().HaveCount(2);
        visit.Diagnoses.Should().Contain(d => d.Laterality == Laterality.OD);
        visit.Diagnoses.Should().Contain(d => d.Laterality == Laterality.OS);
    }

    [Fact]
    public async Task Handle_SignedVisit_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        visit.SignOff(Guid.NewGuid());
        var command = new AddVisitDiagnosisCommand(
            visit.Id, "H04.121", "Dry Eye", "Kho mat",
            (int)Laterality.OD, (int)DiagnosisRole.Primary, 1);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddVisitDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_FirstDiagnosis_SetAsPrimary()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();
        var command = new AddVisitDiagnosisCommand(
            visit.Id, "H40.11", "Open angle glaucoma", "Glaucom goc mo",
            (int)Laterality.OD, (int)DiagnosisRole.Primary, 1);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddVisitDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Diagnoses.First().Role.Should().Be(DiagnosisRole.Primary);
    }

    [Fact]
    public async Task Handle_SubsequentDiagnosis_SetAsSecondary()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateEditableVisit();

        // Add first diagnosis manually
        var firstDiag = VisitDiagnosis.Create(
            visit.Id, "H04.121", "Dry Eye", "Kho mat",
            Laterality.OD, DiagnosisRole.Primary, 1);
        visit.AddDiagnosis(firstDiag);

        var command = new AddVisitDiagnosisCommand(
            visit.Id, "H40.11", "Glaucoma", "Glaucom",
            (int)Laterality.OD, (int)DiagnosisRole.Secondary, 2);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddVisitDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.Diagnoses.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidValidator();
        var command = new AddVisitDiagnosisCommand(
            Guid.NewGuid(), "H04.121", "Dry Eye", "Kho mat",
            (int)Laterality.OD, (int)DiagnosisRole.Primary, 1);

        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await AddVisitDiagnosisHandler.Handle(
            command, _visitRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
