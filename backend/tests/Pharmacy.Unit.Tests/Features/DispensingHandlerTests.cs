using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Pharmacy.Application.Features.Dispensing;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for Dispensing handlers: DispenseDrugs, GetPendingPrescriptions, GetDispensingHistory.
/// PHR-05: Pharmacist can dispense drugs against HIS prescription with auto stock deduction.
/// PHR-07: System enforces 7-day prescription validity and warns on expired prescriptions.
/// </summary>
public class DispensingHandlerTests
{
    private readonly IDispensingRepository _dispensingRepository = Substitute.For<IDispensingRepository>();
    private readonly IDrugBatchRepository _drugBatchRepository = Substitute.For<IDrugBatchRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IValidator<DispenseDrugsCommand> _validator = Substitute.For<IValidator<DispenseDrugsCommand>>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultPrescriptionId = Guid.NewGuid();
    private static readonly Guid DefaultVisitId = Guid.NewGuid();
    private static readonly Guid DefaultPatientId = Guid.NewGuid();
    private static readonly Guid DefaultDrugId = Guid.NewGuid();
    private static readonly Guid DefaultBatchId = Guid.NewGuid();
    private static readonly Guid DefaultItemId = Guid.NewGuid();

    public DispensingHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<DispenseDrugsCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static List<DrugBatch> CreateTestBatches(int totalQty = 100)
    {
        // Create a batch with enough stock for tests
        var batch = DrugBatch.Create(
            drugCatalogItemId: DefaultDrugId,
            supplierId: Guid.NewGuid(),
            batchNumber: "BN2026001",
            expiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            quantity: totalQty,
            purchasePrice: 50000m);

        // Use reflection to set the Id so we can use DefaultBatchId
        var idProp = typeof(Entity).GetProperty("Id");
        idProp?.GetSetMethod(true)?.Invoke(batch, [DefaultBatchId]);

        return [batch];
    }

    private static DispenseDrugsCommand CreateValidCommand(bool withinValidity = true) =>
        new(
            PrescriptionId: DefaultPrescriptionId,
            VisitId: DefaultVisitId,
            PatientId: DefaultPatientId,
            PatientName: "Nguyễn Văn An",
            PrescribedAt: withinValidity
                ? DateTime.UtcNow.AddDays(-2)  // 2 days ago = still valid
                : DateTime.UtcNow.AddDays(-10), // 10 days ago = expired
            OverrideReason: null,
            Lines:
            [
                new DispenseLineInput(
                    PrescriptionItemId: DefaultItemId,
                    DrugCatalogItemId: DefaultDrugId,
                    DrugName: "Tobramycin 0.3%",
                    Quantity: 5,
                    IsOffCatalog: false,
                    Skip: false,
                    ManualBatches: null)
            ]);

    #region DispenseDrugs Tests

    [Fact]
    public async Task DispenseDrugs_ValidPrescription_CreatesRecordWithFEFO()
    {
        // Arrange
        SetupValidValidator();
        var batches = CreateTestBatches(100);
        var command = CreateValidCommand(withinValidity: true);

        _dispensingRepository.GetByPrescriptionIdAsync(command.PrescriptionId, Arg.Any<CancellationToken>())
            .Returns((DispensingRecord?)null);
        _drugBatchRepository.GetAvailableBatchesFEFOAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns(batches);

        // Act
        var result = await DispenseDrugsHandler.Handle(
            command, _dispensingRepository, _drugBatchRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _dispensingRepository.Received(1).Add(Arg.Any<DispensingRecord>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispenseDrugs_ExpiredRx_WithoutOverride_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var command = CreateValidCommand(withinValidity: false);  // 10 days ago = expired

        _dispensingRepository.GetByPrescriptionIdAsync(command.PrescriptionId, Arg.Any<CancellationToken>())
            .Returns((DispensingRecord?)null);

        // Act
        var result = await DispenseDrugsHandler.Handle(
            command, _dispensingRepository, _drugBatchRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Expired");
        _dispensingRepository.DidNotReceive().Add(Arg.Any<DispensingRecord>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispenseDrugs_ExpiredRx_WithOverrideReason_Succeeds()
    {
        // Arrange
        SetupValidValidator();
        var batches = CreateTestBatches(100);
        var command = CreateValidCommand(withinValidity: false) with
        {
            OverrideReason = "Bệnh nhân không thể đến sớm hơn do công tác xa."
        };

        _dispensingRepository.GetByPrescriptionIdAsync(command.PrescriptionId, Arg.Any<CancellationToken>())
            .Returns((DispensingRecord?)null);
        _drugBatchRepository.GetAvailableBatchesFEFOAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns(batches);

        // Act
        var result = await DispenseDrugsHandler.Handle(
            command, _dispensingRepository, _drugBatchRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dispensingRepository.Received(1).Add(Arg.Is<DispensingRecord>(r => r.OverrideReason != null));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispenseDrugs_AlreadyDispensed_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var command = CreateValidCommand();

        // Simulate that a dispensing record already exists for this prescription
        var existingRecord = DispensingRecord.Create(
            prescriptionId: command.PrescriptionId,
            visitId: command.VisitId,
            patientId: command.PatientId,
            patientName: command.PatientName,
            dispensedById: Guid.NewGuid(),
            overrideReason: null,
            branchId: new BranchId(DefaultBranchId));

        _dispensingRepository.GetByPrescriptionIdAsync(command.PrescriptionId, Arg.Any<CancellationToken>())
            .Returns(existingRecord);

        // Act
        var result = await DispenseDrugsHandler.Handle(
            command, _dispensingRepository, _drugBatchRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("AlreadyDispensed");
        _dispensingRepository.DidNotReceive().Add(Arg.Any<DispensingRecord>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispenseDrugs_InsufficientStock_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var command = CreateValidCommand(withinValidity: true) with
        {
            Lines =
            [
                new DispenseLineInput(
                    PrescriptionItemId: DefaultItemId,
                    DrugCatalogItemId: DefaultDrugId,
                    DrugName: "Tobramycin 0.3%",
                    Quantity: 1000,  // Much more than available
                    IsOffCatalog: false,
                    Skip: false,
                    ManualBatches: null)
            ]
        };

        _dispensingRepository.GetByPrescriptionIdAsync(command.PrescriptionId, Arg.Any<CancellationToken>())
            .Returns((DispensingRecord?)null);
        // Only 10 units available, need 1000
        _drugBatchRepository.GetAvailableBatchesFEFOAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns(CreateTestBatches(10));

        // Act
        var result = await DispenseDrugsHandler.Handle(
            command, _dispensingRepository, _drugBatchRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Tobramycin 0.3%");
        _dispensingRepository.DidNotReceive().Add(Arg.Any<DispensingRecord>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispenseDrugs_SkippedLine_RecordsSkipStatus()
    {
        // Arrange
        SetupValidValidator();
        var command = CreateValidCommand(withinValidity: true) with
        {
            Lines =
            [
                new DispenseLineInput(
                    PrescriptionItemId: DefaultItemId,
                    DrugCatalogItemId: DefaultDrugId,
                    DrugName: "Tobramycin 0.3%",
                    Quantity: 5,
                    IsOffCatalog: false,
                    Skip: true,    // Skip this line
                    ManualBatches: null)
            ]
        };

        _dispensingRepository.GetByPrescriptionIdAsync(command.PrescriptionId, Arg.Any<CancellationToken>())
            .Returns((DispensingRecord?)null);

        // Act
        var result = await DispenseDrugsHandler.Handle(
            command, _dispensingRepository, _drugBatchRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dispensingRepository.Received(1).Add(Arg.Is<DispensingRecord>(r =>
            r.Lines.Any(l => l.Status == DispensingStatus.Skipped)));
        // No batch deduction for skipped lines
        await _drugBatchRepository.DidNotReceive().GetAvailableBatchesFEFOAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispenseDrugs_BatchDeductionApplied_AcrossMultipleBatches()
    {
        // Arrange
        SetupValidValidator();

        // Create two small batches that together satisfy the demand
        var batch1 = DrugBatch.Create(DefaultDrugId, Guid.NewGuid(), "BN001", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)), 3, 50000m);
        var batch2 = DrugBatch.Create(DefaultDrugId, Guid.NewGuid(), "BN002", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), 5, 55000m);
        var batches = new List<DrugBatch> { batch1, batch2 };

        var command = CreateValidCommand(withinValidity: true) with
        {
            Lines =
            [
                new DispenseLineInput(
                    PrescriptionItemId: DefaultItemId,
                    DrugCatalogItemId: DefaultDrugId,
                    DrugName: "Tobramycin 0.3%",
                    Quantity: 7,  // Requires both batches: 3 + 4
                    IsOffCatalog: false,
                    Skip: false,
                    ManualBatches: null)
            ]
        };

        _dispensingRepository.GetByPrescriptionIdAsync(command.PrescriptionId, Arg.Any<CancellationToken>())
            .Returns((DispensingRecord?)null);
        _drugBatchRepository.GetAvailableBatchesFEFOAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns(batches);

        // Act
        var result = await DispenseDrugsHandler.Handle(
            command, _dispensingRepository, _drugBatchRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // FEFO allocator should have allocated from both batches
        _dispensingRepository.Received(1).Add(Arg.Is<DispensingRecord>(r =>
            r.Lines.Any(l => l.BatchDeductions.Count == 2)));
    }

    #endregion

    #region GetPendingPrescriptions Tests

    [Fact]
    public async Task GetPendingPrescriptions_ReturnsUndispensed()
    {
        // Arrange
        var items = new List<PendingPrescriptionDto>
        {
            new(
                PrescriptionId: DefaultPrescriptionId,
                VisitId: DefaultVisitId,
                PatientId: DefaultPatientId,
                PatientName: "Nguyễn Văn An",
                PrescribedAt: DateTime.UtcNow.AddDays(-1),
                IsExpired: false,
                DaysRemaining: 6,
                Items: new List<PendingPrescriptionItemDto>
                {
                    new(
                        PrescriptionItemId: DefaultItemId,
                        DrugCatalogItemId: DefaultDrugId,
                        DrugName: "Tobramycin 0.3%",
                        Quantity: 5,
                        Unit: "Chai",
                        Dosage: "2 drops x 4 times/day",
                        IsOffCatalog: false)
                })
        };

        _dispensingRepository.GetPendingPrescriptionsAsync(null, Arg.Any<CancellationToken>())
            .Returns(items);

        var query = new GetPendingPrescriptionsQuery();

        // Act
        var result = await GetPendingPrescriptionsHandler.Handle(
            query, _dispensingRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PrescriptionId.Should().Be(DefaultPrescriptionId);
        result.Value[0].IsExpired.Should().BeFalse();
    }

    #endregion

    #region GetDispensingHistory Tests

    [Fact]
    public async Task GetDispensingHistory_ReturnsPaginated()
    {
        // Arrange
        var items = new List<DispensingRecordDto>
        {
            new(
                Id: Guid.NewGuid(),
                PrescriptionId: DefaultPrescriptionId,
                VisitId: DefaultVisitId,
                PatientId: DefaultPatientId,
                PatientName: "Nguyễn Văn An",
                DispensedAt: DateTime.UtcNow.AddHours(-2),
                OverrideReason: null,
                Lines: new List<DispensingLineDto>())
        };

        _dispensingRepository.GetHistoryAsync(1, 20, null, Arg.Any<CancellationToken>())
            .Returns((items, 1));

        var query = new GetDispensingHistoryQuery(Page: 1, PageSize: 20, PatientId: null);

        // Act
        var result = await GetDispensingHistoryHandler.Handle(
            query, _dispensingRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].PatientName.Should().Be("Nguyễn Văn An");
    }

    #endregion
}
