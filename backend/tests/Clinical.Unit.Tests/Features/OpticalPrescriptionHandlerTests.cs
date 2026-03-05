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

public class OpticalPrescriptionHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<AddOpticalPrescriptionCommand> _addValidator = Substitute.For<IValidator<AddOpticalPrescriptionCommand>>();
    private readonly IValidator<UpdateOpticalPrescriptionCommand> _updateValidator = Substitute.For<IValidator<UpdateOpticalPrescriptionCommand>>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private void SetupValidAddValidator()
    {
        _addValidator.ValidateAsync(Arg.Any<AddOpticalPrescriptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidUpdateValidator()
    {
        _updateValidator.ValidateAsync(Arg.Any<UpdateOpticalPrescriptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static Visit CreateEditableVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
    }

    [Fact]
    public async Task AddOpticalPrescription_WithAllFields_ReturnsSuccess()
    {
        // Arrange
        SetupValidAddValidator();
        var visit = CreateEditableVisit();
        var command = new AddOpticalPrescriptionCommand(
            visit.Id,
            OdSph: -2.50m, OdCyl: -0.75m, OdAxis: 90, OdAdd: 1.50m,
            OsSph: -3.00m, OsCyl: -1.00m, OsAxis: 85, OsAdd: 1.50m,
            FarPd: 63.5m, NearPd: 60.5m,
            NearOdSph: -1.00m, NearOdCyl: -0.50m, NearOdAxis: 90,
            NearOsSph: -1.50m, NearOsCyl: -0.75m, NearOsAxis: 85,
            LensType: (int)LensType.Progressive, Notes: "Progressive lenses recommended");

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddOpticalPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        visit.OpticalPrescriptions.Should().HaveCount(1);
        var rx = visit.OpticalPrescriptions.First();
        rx.OdSph.Should().Be(-2.50m);
        rx.OdCyl.Should().Be(-0.75m);
        rx.OdAxis.Should().Be(90);
        rx.OdAdd.Should().Be(1.50m);
        rx.OsSph.Should().Be(-3.00m);
        rx.OsCyl.Should().Be(-1.00m);
        rx.OsAxis.Should().Be(85);
        rx.OsAdd.Should().Be(1.50m);
        rx.FarPd.Should().Be(63.5m);
        rx.NearPd.Should().Be(60.5m);
        rx.NearOdSph.Should().Be(-1.00m);
        rx.NearOdCyl.Should().Be(-0.50m);
        rx.NearOdAxis.Should().Be(90);
        rx.NearOsSph.Should().Be(-1.50m);
        rx.NearOsCyl.Should().Be(-0.75m);
        rx.NearOsAxis.Should().Be(85);
        rx.LensType.Should().Be(LensType.Progressive);
        rx.Notes.Should().Be("Progressive lenses recommended");
        _visitRepository.Received(1).AddOpticalPrescription(Arg.Any<OpticalPrescription>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOpticalPrescription_ReplacesExisting_OnlyOnePerVisit()
    {
        // Arrange
        SetupValidAddValidator();
        var visit = CreateEditableVisit();

        // Add first optical Rx
        var firstCommand = new AddOpticalPrescriptionCommand(
            visit.Id,
            OdSph: -1.00m, OdCyl: null, OdAxis: null, OdAdd: null,
            OsSph: -1.25m, OsCyl: null, OsAxis: null, OsAdd: null,
            FarPd: 62m, NearPd: null,
            NearOdSph: null, NearOdCyl: null, NearOdAxis: null,
            NearOsSph: null, NearOsCyl: null, NearOsAxis: null,
            LensType: (int)LensType.SingleVision, Notes: null);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        await AddOpticalPrescriptionHandler.Handle(
            firstCommand, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        visit.OpticalPrescriptions.Should().HaveCount(1);
        var firstRxId = visit.OpticalPrescriptions.First().Id;

        // Act -- add second optical Rx (should replace first)
        var secondCommand = new AddOpticalPrescriptionCommand(
            visit.Id,
            OdSph: -2.00m, OdCyl: -0.50m, OdAxis: 180, OdAdd: null,
            OsSph: -2.25m, OsCyl: -0.75m, OsAxis: 175, OsAdd: null,
            FarPd: 64m, NearPd: null,
            NearOdSph: null, NearOdCyl: null, NearOdAxis: null,
            NearOsSph: null, NearOsCyl: null, NearOsAxis: null,
            LensType: (int)LensType.SingleVision, Notes: "Updated prescription");

        var result = await AddOpticalPrescriptionHandler.Handle(
            secondCommand, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        visit.OpticalPrescriptions.Should().HaveCount(1); // Still 1 -- replaced
        var rx = visit.OpticalPrescriptions.First();
        rx.OdSph.Should().Be(-2.00m);
        rx.Notes.Should().Be("Updated prescription");
    }

    [Fact]
    public async Task AddOpticalPrescription_OnSignedVisit_ReturnsError()
    {
        // Arrange
        SetupValidAddValidator();
        var visit = CreateEditableVisit();
        visit.SignOff(Guid.NewGuid()); // Make it signed

        var command = new AddOpticalPrescriptionCommand(
            visit.Id,
            OdSph: -1.00m, OdCyl: null, OdAxis: null, OdAdd: null,
            OsSph: -1.25m, OsCyl: null, OsAxis: null, OsAdd: null,
            FarPd: 62m, NearPd: null,
            NearOdSph: null, NearOdCyl: null, NearOdAxis: null,
            NearOsSph: null, NearOsCyl: null, NearOsAxis: null,
            LensType: (int)LensType.SingleVision, Notes: null);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddOpticalPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task AddOpticalPrescription_WithLensType_StoresCorrectly()
    {
        // Arrange
        SetupValidAddValidator();
        var visit = CreateEditableVisit();

        var command = new AddOpticalPrescriptionCommand(
            visit.Id,
            OdSph: -1.50m, OdCyl: null, OdAxis: null, OdAdd: 2.00m,
            OsSph: -1.75m, OsCyl: null, OsAxis: null, OsAdd: 2.00m,
            FarPd: 63m, NearPd: 60m,
            NearOdSph: null, NearOdCyl: null, NearOdAxis: null,
            NearOsSph: null, NearOsCyl: null, NearOsAxis: null,
            LensType: (int)LensType.Bifocal, Notes: "Bifocal for reading");

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        // Act
        var result = await AddOpticalPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var rx = visit.OpticalPrescriptions.First();
        rx.LensType.Should().Be(LensType.Bifocal);
        rx.OdAdd.Should().Be(2.00m);
        rx.OsAdd.Should().Be(2.00m);
        rx.FarPd.Should().Be(63m);
        rx.NearPd.Should().Be(60m);
    }

    [Fact]
    public async Task UpdateOpticalPrescription_UpdatesFields()
    {
        // Arrange
        SetupValidAddValidator();
        SetupValidUpdateValidator();
        var visit = CreateEditableVisit();

        // First create an optical Rx via Add
        var addCommand = new AddOpticalPrescriptionCommand(
            visit.Id,
            OdSph: -1.00m, OdCyl: null, OdAxis: null, OdAdd: null,
            OsSph: -1.25m, OsCyl: null, OsAxis: null, OsAdd: null,
            FarPd: 62m, NearPd: null,
            NearOdSph: null, NearOdCyl: null, NearOdAxis: null,
            NearOsSph: null, NearOsCyl: null, NearOsAxis: null,
            LensType: (int)LensType.SingleVision, Notes: null);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        await AddOpticalPrescriptionHandler.Handle(
            addCommand, _visitRepository, _unitOfWork, _addValidator, CancellationToken.None);

        var rxId = visit.OpticalPrescriptions.First().Id;

        // Act -- update it
        var updateCommand = new UpdateOpticalPrescriptionCommand(
            visit.Id, rxId,
            OdSph: -2.00m, OdCyl: -0.50m, OdAxis: 90, OdAdd: 1.50m,
            OsSph: -2.25m, OsCyl: -0.75m, OsAxis: 85, OsAdd: 1.50m,
            FarPd: 64m, NearPd: 61m,
            NearOdSph: -0.50m, NearOdCyl: null, NearOdAxis: null,
            NearOsSph: -0.75m, NearOsCyl: null, NearOsAxis: null,
            LensType: (int)LensType.Progressive, Notes: "Changed to progressive");

        var result = await UpdateOpticalPrescriptionHandler.Handle(
            updateCommand, _visitRepository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var rx = visit.OpticalPrescriptions.First();
        rx.OdSph.Should().Be(-2.00m);
        rx.OdCyl.Should().Be(-0.50m);
        rx.OdAxis.Should().Be(90);
        rx.OdAdd.Should().Be(1.50m);
        rx.OsSph.Should().Be(-2.25m);
        rx.OsCyl.Should().Be(-0.75m);
        rx.LensType.Should().Be(LensType.Progressive);
        rx.Notes.Should().Be("Changed to progressive");
        rx.FarPd.Should().Be(64m);
        rx.NearPd.Should().Be(61m);
        rx.NearOdSph.Should().Be(-0.50m);
        rx.NearOsSph.Should().Be(-0.75m);
    }

    [Fact]
    public async Task UpdateOpticalPrescription_VisitNotFound_ReturnsError()
    {
        // Arrange
        SetupValidUpdateValidator();
        var command = new UpdateOpticalPrescriptionCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            OdSph: -1.00m, OdCyl: null, OdAxis: null, OdAdd: null,
            OsSph: -1.25m, OsCyl: null, OsAxis: null, OsAdd: null,
            FarPd: 62m, NearPd: null,
            NearOdSph: null, NearOdCyl: null, NearOdAxis: null,
            NearOsSph: null, NearOsCyl: null, NearOsAxis: null,
            LensType: (int)LensType.SingleVision, Notes: null);

        _visitRepository.GetByIdWithDetailsAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await UpdateOpticalPrescriptionHandler.Handle(
            command, _visitRepository, _unitOfWork, _updateValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
