using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Patient.Contracts.Dtos;
using Patient.Contracts.Enums;
using Shared.Domain;
using Wolverine;

namespace Clinical.Unit.Tests.Features;

public class DrugPrescriptionHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<AddDrugPrescriptionCommand> _addValidator = Substitute.For<IValidator<AddDrugPrescriptionCommand>>();
    private readonly IValidator<UpdateDrugPrescriptionCommand> _updateValidator = Substitute.For<IValidator<UpdateDrugPrescriptionCommand>>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private void SetupValidAddValidator()
    {
        _addValidator.ValidateAsync(Arg.Any<AddDrugPrescriptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidUpdateValidator()
    {
        _updateValidator.ValidateAsync(Arg.Any<UpdateDrugPrescriptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static Visit CreateEditableVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
    }

    private static PrescriptionItemInput CreateCatalogItemInput(Guid catalogId, bool hasAllergyWarning = false)
    {
        return new PrescriptionItemInput(
            DrugCatalogItemId: catalogId,
            DrugName: "Tobramycin 0.3%",
            GenericName: "Tobramycin",
            Strength: "0.3%",
            Form: 0, // Drops
            Route: 0, // Topical
            Dosage: "1 drop, 4 times/day",
            DosageOverride: null,
            Quantity: 1,
            Unit: "bottle",
            Frequency: "4 times/day",
            DurationDays: 7,
            HasAllergyWarning: hasAllergyWarning);
    }

    private static PrescriptionItemInput CreateOffCatalogItemInput()
    {
        return new PrescriptionItemInput(
            DrugCatalogItemId: null,
            DrugName: "Custom Eye Drop",
            GenericName: null,
            Strength: null,
            Form: 0, // Drops
            Route: 0, // Topical
            Dosage: "As directed",
            DosageOverride: null,
            Quantity: 1,
            Unit: "bottle",
            Frequency: null,
            DurationDays: null,
            HasAllergyWarning: false);
    }

    // ===== AddDrugPrescription Tests =====

    [Fact]
    public async Task AddDrugPrescription_WithCatalogItem_ReturnsSuccess()
    {
        // Arrange
        SetupValidAddValidator();
        var visit = CreateEditableVisit();
        var catalogId = Guid.NewGuid();
        var command = new AddDrugPrescriptionCommand(
            visit.Id, "Take after meals",
            [CreateCatalogItemInput(catalogId)]);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddDrugPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        visit.DrugPrescriptions.Should().HaveCount(1);
        var rx = visit.DrugPrescriptions.First();
        rx.Items.Should().HaveCount(1);
        rx.Items.First().DrugCatalogItemId.Should().Be(catalogId);
        rx.Items.First().IsOffCatalog.Should().BeFalse();
    }

    [Fact]
    public async Task AddDrugPrescription_WithOffCatalogDrug_SetsIsOffCatalog()
    {
        // Arrange
        SetupValidAddValidator();
        var visit = CreateEditableVisit();
        var command = new AddDrugPrescriptionCommand(
            visit.Id, null,
            [CreateOffCatalogItemInput()]);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddDrugPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var rx = visit.DrugPrescriptions.First();
        rx.Items.First().IsOffCatalog.Should().BeTrue();
        rx.Items.First().DrugCatalogItemId.Should().BeNull();
    }

    [Fact]
    public async Task AddDrugPrescription_OnSignedVisit_ReturnsError()
    {
        // Arrange
        SetupValidAddValidator();
        var visit = CreateEditableVisit();
        visit.SignOff(Guid.NewGuid());
        var command = new AddDrugPrescriptionCommand(
            visit.Id, null,
            [CreateCatalogItemInput(Guid.NewGuid())]);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddDrugPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task AddDrugPrescription_WithAllergyWarning_SetsFlag()
    {
        // Arrange
        SetupValidAddValidator();
        var visit = CreateEditableVisit();
        var command = new AddDrugPrescriptionCommand(
            visit.Id, null,
            [CreateCatalogItemInput(Guid.NewGuid(), hasAllergyWarning: true)]);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddDrugPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var rx = visit.DrugPrescriptions.First();
        rx.Items.First().HasAllergyWarning.Should().BeTrue();
    }

    [Fact]
    public async Task AddDrugPrescription_VisitNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidAddValidator();
        var command = new AddDrugPrescriptionCommand(
            Guid.NewGuid(), null,
            [CreateCatalogItemInput(Guid.NewGuid())]);

        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await AddDrugPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    // ===== UpdateDrugPrescription Tests =====

    [Fact]
    public async Task UpdateDrugPrescription_UpdatesNotes()
    {
        // Arrange
        SetupValidUpdateValidator();
        var visit = CreateEditableVisit();
        var prescription = DrugPrescription.Create(visit.Id, "Old notes");
        visit.AddDrugPrescription(prescription);

        var command = new UpdateDrugPrescriptionCommand(
            visit.Id, prescription.Id, "Updated notes with new advice");

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await UpdateDrugPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        prescription.Notes.Should().Be("Updated notes with new advice");
    }

    // ===== RemoveDrugPrescription Tests =====

    [Fact]
    public async Task RemoveDrugPrescription_RemovesPrescription()
    {
        // Arrange
        var visit = CreateEditableVisit();
        var prescription = DrugPrescription.Create(visit.Id, "Test notes");
        visit.AddDrugPrescription(prescription);

        var command = new RemoveDrugPrescriptionCommand(visit.Id, prescription.Id);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await RemoveDrugPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.DrugPrescriptions.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveDrugPrescription_OnSignedVisit_ReturnsError()
    {
        // Arrange
        var visit = CreateEditableVisit();
        var prescription = DrugPrescription.Create(visit.Id, "Test notes");
        visit.AddDrugPrescription(prescription);
        visit.SignOff(Guid.NewGuid());

        var command = new RemoveDrugPrescriptionCommand(visit.Id, prescription.Id);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await RemoveDrugPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // ===== CheckDrugAllergy Tests =====

    [Fact]
    public async Task CheckDrugAllergy_MatchingAllergyName_ReturnsMatches()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var allergies = new List<AllergyDto>
        {
            new(Guid.NewGuid(), "Tobramycin", AllergySeverity.Severe),
            new(Guid.NewGuid(), "Penicillin", AllergySeverity.Moderate)
        };

        _messageBus.InvokeAsync<List<AllergyDto>>(
            Arg.Any<GetPatientAllergiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(allergies);

        var query = new CheckDrugAllergyQuery(patientId, "Tobramycin 0.3%", "Tobramycin");

        // Act
        var result = await CheckDrugAllergyHandler.Handle(query, _messageBus, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Tobramycin");
    }

    [Fact]
    public async Task CheckDrugAllergy_MatchingGenericName_ReturnsMatches()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var allergies = new List<AllergyDto>
        {
            new(Guid.NewGuid(), "Amoxicillin", AllergySeverity.Severe)
        };

        _messageBus.InvokeAsync<List<AllergyDto>>(
            Arg.Any<GetPatientAllergiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(allergies);

        var query = new CheckDrugAllergyQuery(patientId, "Augmentin", "Amoxicillin");

        // Act
        var result = await CheckDrugAllergyHandler.Handle(query, _messageBus, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Amoxicillin");
    }

    [Fact]
    public async Task CheckDrugAllergy_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var allergies = new List<AllergyDto>
        {
            new(Guid.NewGuid(), "Penicillin", AllergySeverity.Severe)
        };

        _messageBus.InvokeAsync<List<AllergyDto>>(
            Arg.Any<GetPatientAllergiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(allergies);

        var query = new CheckDrugAllergyQuery(patientId, "Tobramycin 0.3%", "Tobramycin");

        // Act
        var result = await CheckDrugAllergyHandler.Handle(query, _messageBus, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckDrugAllergy_CaseInsensitive_ReturnsMatches()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var allergies = new List<AllergyDto>
        {
            new(Guid.NewGuid(), "tobramycin", AllergySeverity.Moderate)
        };

        _messageBus.InvokeAsync<List<AllergyDto>>(
            Arg.Any<GetPatientAllergiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(allergies);

        var query = new CheckDrugAllergyQuery(patientId, "TOBRAMYCIN Eye Drops", "Tobramycin");

        // Act
        var result = await CheckDrugAllergyHandler.Handle(query, _messageBus, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("tobramycin");
    }
}
