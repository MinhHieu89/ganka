using FluentValidation;
using FluentValidation.Results;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Features;
using Treatment.Contracts.Dtos;

namespace Treatment.Unit.Tests.Features;

public class ModifyPackageHandlerTests
{
    private static readonly BranchId DefaultBranchId =
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private readonly ITreatmentPackageRepository _packageRepository = Substitute.For<ITreatmentPackageRepository>();
    private readonly ITreatmentProtocolRepository _protocolRepository = Substitute.For<ITreatmentProtocolRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    // Validators
    private readonly IValidator<ModifyTreatmentPackageCommand> _modifyValidator =
        Substitute.For<IValidator<ModifyTreatmentPackageCommand>>();

    private readonly IValidator<SwitchTreatmentTypeCommand> _switchValidator =
        Substitute.For<IValidator<SwitchTreatmentTypeCommand>>();

    private readonly IValidator<PauseTreatmentPackageCommand> _pauseValidator =
        Substitute.For<IValidator<PauseTreatmentPackageCommand>>();

    public ModifyPackageHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.BranchId.Returns(DefaultBranchId.Value);
    }

    private void SetupValidModifyValidator()
    {
        _modifyValidator.ValidateAsync(Arg.Any<ModifyTreatmentPackageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidSwitchValidator()
    {
        _switchValidator.ValidateAsync(Arg.Any<SwitchTreatmentTypeCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidPauseValidator()
    {
        _pauseValidator.ValidateAsync(Arg.Any<PauseTreatmentPackageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private TreatmentPackage CreateActivePackage(int totalSessions = 4, int completedSessions = 0)
    {
        var package = TreatmentPackage.Create(
            protocolTemplateId: Guid.NewGuid(),
            patientId: Guid.NewGuid(),
            patientName: "Test Patient",
            treatmentType: TreatmentType.IPL,
            totalSessions: totalSessions,
            pricingMode: PricingMode.PerPackage,
            packagePrice: 2_000_000m,
            sessionPrice: 500_000m,
            minIntervalDays: 14,
            parametersJson: "{\"energy\":15}",
            visitId: null,
            createdById: Guid.NewGuid(),
            branchId: DefaultBranchId);

        // Record completed sessions if needed
        for (int i = 0; i < completedSessions; i++)
        {
            package.RecordSession(
                parametersJson: "{\"energy\":15}",
                osdiScore: 20m,
                osdiSeverity: "Mild",
                clinicalNotes: "Session ok",
                performedById: Guid.NewGuid(),
                visitId: null,
                scheduledAt: null,
                intervalOverrideReason: i > 0 ? "Test setup: back-to-back sessions" : null,
                consumables: []);
        }

        return package;
    }

    private TreatmentProtocol CreateProtocolTemplate(TreatmentType type = TreatmentType.LLLT)
    {
        return TreatmentProtocol.Create(
            name: "LLLT 6-session",
            treatmentType: type,
            defaultSessionCount: 6,
            pricingMode: PricingMode.PerSession,
            defaultPackagePrice: 0m,
            defaultSessionPrice: 300_000m,
            minIntervalDays: 7,
            maxIntervalDays: 14,
            defaultParametersJson: "{\"wavelength\":850}",
            cancellationDeductionPercent: 15m,
            description: "Standard LLLT protocol",
            branchId: DefaultBranchId);
    }

    // ==================== ModifyTreatmentPackage Tests ====================

    [Fact]
    public async Task Modify_ActivePackage_CreatesVersionAndAppliesChanges()
    {
        // Arrange
        SetupValidModifyValidator();
        var package = CreateActivePackage(totalSessions: 4);
        var command = new ModifyTreatmentPackageCommand(
            PackageId: package.Id,
            TotalSessions: 6,
            ParametersJson: "{\"energy\":20}",
            MinIntervalDays: 21,
            Reason: "Patient needs more sessions");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await ModifyTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _modifyValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalSessions.Should().Be(6);
        result.Value.ParametersJson.Should().Be("{\"energy\":20}");
        result.Value.MinIntervalDays.Should().Be(21);
        package.Versions.Should().HaveCount(1);
        package.Versions[0].Reason.Should().Be("Patient needs more sessions");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Modify_PausedPackage_CreatesVersionAndAppliesChanges()
    {
        // Arrange
        SetupValidModifyValidator();
        var package = CreateActivePackage(totalSessions: 4);
        package.Pause();
        var command = new ModifyTreatmentPackageCommand(
            PackageId: package.Id,
            TotalSessions: 5,
            ParametersJson: null,
            MinIntervalDays: null,
            Reason: "Adjusted while paused");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await ModifyTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _modifyValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalSessions.Should().Be(5);
        package.Versions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Modify_NonExistentPackage_ReturnsNotFound()
    {
        // Arrange
        SetupValidModifyValidator();
        var packageId = Guid.NewGuid();
        var command = new ModifyTreatmentPackageCommand(
            PackageId: packageId,
            TotalSessions: 6,
            ParametersJson: null,
            MinIntervalDays: null,
            Reason: "Adjust");

        _packageRepository.GetByIdAsync(packageId, Arg.Any<CancellationToken>()).Returns((TreatmentPackage?)null);

        // Act
        var result = await ModifyTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _modifyValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Modify_CompletedPackage_ReturnsError()
    {
        // Arrange
        SetupValidModifyValidator();
        var package = CreateActivePackage(totalSessions: 1);
        // Complete the single session to auto-complete the package
        package.RecordSession("{}", 10m, "Normal", "Done", Guid.NewGuid(), null, null, null, []);

        var command = new ModifyTreatmentPackageCommand(
            PackageId: package.Id,
            TotalSessions: 4,
            ParametersJson: null,
            MinIntervalDays: null,
            Reason: "Want more");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await ModifyTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _modifyValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Modify_VersionSnapshotCapturesPreviousAndCurrentState()
    {
        // Arrange
        SetupValidModifyValidator();
        var package = CreateActivePackage(totalSessions: 4);
        var command = new ModifyTreatmentPackageCommand(
            PackageId: package.Id,
            TotalSessions: 6,
            ParametersJson: null,
            MinIntervalDays: null,
            Reason: "Need more sessions");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await ModifyTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _modifyValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var version = package.Versions[0];
        version.PreviousJson.Should().Contain("4"); // old TotalSessions
        version.CurrentJson.Should().Contain("6"); // new TotalSessions
        version.ChangeDescription.Should().Contain("Session count changed from 4 to 6");
    }

    // ==================== SwitchTreatmentType Tests ====================

    [Fact]
    public async Task Switch_ActivePackage_MarksOldAsSwitchedAndCreatesNew()
    {
        // Arrange
        SetupValidSwitchValidator();
        var oldPackage = CreateActivePackage(totalSessions: 4, completedSessions: 2);
        var newTemplate = CreateProtocolTemplate(TreatmentType.LLLT);
        var command = new SwitchTreatmentTypeCommand(
            PackageId: oldPackage.Id,
            NewProtocolTemplateId: newTemplate.Id,
            Reason: "Switching to LLLT");

        _packageRepository.GetByIdAsync(oldPackage.Id, Arg.Any<CancellationToken>()).Returns(oldPackage);
        _protocolRepository.GetByIdAsync(newTemplate.Id, Arg.Any<CancellationToken>()).Returns(newTemplate);

        // Act
        var result = await SwitchTreatmentTypeHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _switchValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        oldPackage.Status.Should().Be(PackageStatus.Switched);
        result.Value.TreatmentType.Should().Be("LLLT");
        result.Value.TotalSessions.Should().Be(2); // remaining sessions = 4 - 2
        result.Value.Status.Should().Be("Active");
        _packageRepository.Received(1).Add(Arg.Any<TreatmentPackage>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Switch_NonExistentPackage_ReturnsNotFound()
    {
        // Arrange
        SetupValidSwitchValidator();
        var packageId = Guid.NewGuid();
        var command = new SwitchTreatmentTypeCommand(
            PackageId: packageId,
            NewProtocolTemplateId: Guid.NewGuid(),
            Reason: "Switch");

        _packageRepository.GetByIdAsync(packageId, Arg.Any<CancellationToken>()).Returns((TreatmentPackage?)null);

        // Act
        var result = await SwitchTreatmentTypeHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _switchValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Switch_NonExistentTemplate_ReturnsNotFound()
    {
        // Arrange
        SetupValidSwitchValidator();
        var package = CreateActivePackage();
        var templateId = Guid.NewGuid();
        var command = new SwitchTreatmentTypeCommand(
            PackageId: package.Id,
            NewProtocolTemplateId: templateId,
            Reason: "Switch");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);
        _protocolRepository.GetByIdAsync(templateId, Arg.Any<CancellationToken>()).Returns((TreatmentProtocol?)null);

        // Act
        var result = await SwitchTreatmentTypeHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _switchValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Switch_CompletedPackage_ReturnsError()
    {
        // Arrange
        SetupValidSwitchValidator();
        var package = CreateActivePackage(totalSessions: 1);
        package.RecordSession("{}", 10m, "Normal", "Done", Guid.NewGuid(), null, null, null, []);
        // Package is now Completed

        var newTemplate = CreateProtocolTemplate();
        var command = new SwitchTreatmentTypeCommand(
            PackageId: package.Id,
            NewProtocolTemplateId: newTemplate.Id,
            Reason: "Switch attempt");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);
        _protocolRepository.GetByIdAsync(newTemplate.Id, Arg.Any<CancellationToken>()).Returns(newTemplate);

        // Act
        var result = await SwitchTreatmentTypeHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _switchValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Switch_InheritsRemainingSessionCount()
    {
        // Arrange
        SetupValidSwitchValidator();
        var oldPackage = CreateActivePackage(totalSessions: 6, completedSessions: 1);
        var newTemplate = CreateProtocolTemplate(TreatmentType.LLLT);
        var command = new SwitchTreatmentTypeCommand(
            PackageId: oldPackage.Id,
            NewProtocolTemplateId: newTemplate.Id,
            Reason: "Switching");

        _packageRepository.GetByIdAsync(oldPackage.Id, Arg.Any<CancellationToken>()).Returns(oldPackage);
        _protocolRepository.GetByIdAsync(newTemplate.Id, Arg.Any<CancellationToken>()).Returns(newTemplate);

        // Act
        var result = await SwitchTreatmentTypeHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _switchValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalSessions.Should().Be(5); // 6 total - 1 completed = 5 remaining
    }

    // ==================== PauseTreatmentPackage Tests ====================

    [Fact]
    public async Task Pause_ActivePackage_SetsStatusToPaused()
    {
        // Arrange
        SetupValidPauseValidator();
        var package = CreateActivePackage();
        var command = new PauseTreatmentPackageCommand(
            PackageId: package.Id,
            Action: PauseAction.Pause);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await PauseTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _pauseValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Paused");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resume_PausedPackage_SetsStatusToActive()
    {
        // Arrange
        SetupValidPauseValidator();
        var package = CreateActivePackage();
        package.Pause();
        var command = new PauseTreatmentPackageCommand(
            PackageId: package.Id,
            Action: PauseAction.Resume);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await PauseTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _pauseValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Resume_ActivePackage_ReturnsError()
    {
        // Arrange
        SetupValidPauseValidator();
        var package = CreateActivePackage();
        var command = new PauseTreatmentPackageCommand(
            PackageId: package.Id,
            Action: PauseAction.Resume);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await PauseTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _pauseValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Pause_CompletedPackage_ReturnsError()
    {
        // Arrange
        SetupValidPauseValidator();
        var package = CreateActivePackage(totalSessions: 1);
        package.RecordSession("{}", 10m, "Normal", "Done", Guid.NewGuid(), null, null, null, []);
        // Package is now Completed

        var command = new PauseTreatmentPackageCommand(
            PackageId: package.Id,
            Action: PauseAction.Pause);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await PauseTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _pauseValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Pause_NonExistentPackage_ReturnsNotFound()
    {
        // Arrange
        SetupValidPauseValidator();
        var packageId = Guid.NewGuid();
        var command = new PauseTreatmentPackageCommand(
            PackageId: packageId,
            Action: PauseAction.Pause);

        _packageRepository.GetByIdAsync(packageId, Arg.Any<CancellationToken>()).Returns((TreatmentPackage?)null);

        // Act
        var result = await PauseTreatmentPackageHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _pauseValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
