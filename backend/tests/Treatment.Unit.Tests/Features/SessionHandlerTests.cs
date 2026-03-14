using Treatment.Application.Features;
using Treatment.Contracts.Dtos;
using FluentValidation;
using FluentValidation.Results;
using Shared.Application;
using Shared.Domain;

namespace Treatment.Unit.Tests.Features;

public class SessionHandlerTests
{
    private readonly ITreatmentPackageRepository _packageRepository = Substitute.For<ITreatmentPackageRepository>();
    private readonly ITreatmentProtocolRepository _protocolRepository = Substitute.For<ITreatmentProtocolRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<RecordTreatmentSessionCommand> _validator = Substitute.For<IValidator<RecordTreatmentSessionCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid DefaultPatientId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid DefaultProtocolId = Guid.Parse("00000000-0000-0000-0000-000000000004");

    public SessionHandlerTests()
    {
        _currentUser.UserId.Returns(DefaultUserId);
        _currentUser.BranchId.Returns(DefaultBranchId);
    }

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<RecordTreatmentSessionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private TreatmentPackage CreateActivePackage(
        int totalSessions = 4,
        int minIntervalDays = 14)
    {
        return TreatmentPackage.Create(
            protocolTemplateId: DefaultProtocolId,
            patientId: DefaultPatientId,
            patientName: "Nguyen Van A",
            treatmentType: TreatmentType.IPL,
            totalSessions: totalSessions,
            pricingMode: PricingMode.PerPackage,
            packagePrice: 4_000_000m,
            sessionPrice: 1_200_000m,
            minIntervalDays: minIntervalDays,
            parametersJson: "{\"energy\":15,\"pulseWidth\":10}",
            visitId: null,
            createdById: DefaultUserId,
            branchId: new BranchId(DefaultBranchId));
    }

    private RecordTreatmentSessionCommand CreateRecordCommand(
        Guid packageId,
        decimal? osdiScore = 25.5m,
        string? osdiSeverity = "Moderate",
        string? intervalOverrideReason = null,
        List<RecordTreatmentSessionCommand.ConsumableInput>? consumables = null)
    {
        return new RecordTreatmentSessionCommand(
            PackageId: packageId,
            ParametersJson: "{\"energy\":15,\"pulseWidth\":10}",
            OsdiScore: osdiScore,
            OsdiSeverity: osdiSeverity,
            ClinicalNotes: "Patient tolerated well",
            PerformedById: DefaultUserId,
            VisitId: null,
            ScheduledAt: null,
            IntervalOverrideReason: intervalOverrideReason,
            Consumables: consumables ?? []);
    }

    // =========================================================================
    // RecordTreatmentSession Tests
    // =========================================================================

    [Fact]
    public async Task RecordSession_ActivePackage_RecordsSessionAndReturnsDto()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage();
        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var command = CreateRecordCommand(package.Id);

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Session.Should().NotBeNull();
        result.Value.Session.SessionNumber.Should().Be(1);
        result.Value.Session.Status.Should().Be("Completed");
        result.Value.Session.OsdiScore.Should().Be(25.5m);
        result.Value.Session.OsdiSeverity.Should().Be("Moderate");
        result.Value.Session.ClinicalNotes.Should().Be("Patient tolerated well");
        result.Value.Warning.Should().BeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordSession_PackageNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidValidator();
        var packageId = Guid.NewGuid();
        _packageRepository.GetByIdAsync(packageId, Arg.Any<CancellationToken>())
            .Returns((TreatmentPackage?)null);

        var command = CreateRecordCommand(packageId);

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task RecordSession_PackageNotActive_ReturnsValidationError()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage();
        package.Pause(); // Status = Paused
        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var command = CreateRecordCommand(package.Id);

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Description.Should().Contain("Active");
    }

    [Fact]
    public async Task RecordSession_WithinMinInterval_WithOverride_ReturnsWarningButStillRecords()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage(totalSessions: 4, minIntervalDays: 14);

        // Record a first session so there's a "last session"
        package.RecordSession(
            "{\"energy\":15}", null, null, null,
            DefaultUserId, null, null, null, []);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        // Must provide override reason since domain enforces interval strictly
        var command = CreateRecordCommand(package.Id,
            intervalOverrideReason: "Patient travelling, needs early session");

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert -- should succeed with a warning (TRT-05: handler-level soft enforcement)
        result.IsSuccess.Should().BeTrue();
        result.Value.Session.SessionNumber.Should().Be(2);
        result.Value.Warning.Should().NotBeNull();
        result.Value.Warning!.MinIntervalDays.Should().Be(14);
        result.Value.Warning.DaysSinceLast.Should().Be(0); // Same day

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordSession_WithOverrideReason_StoresOverrideReason()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage(totalSessions: 4, minIntervalDays: 14);

        // Record first session
        package.RecordSession(
            "{\"energy\":15}", null, null, null,
            DefaultUserId, null, null, null, []);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var command = CreateRecordCommand(
            package.Id,
            intervalOverrideReason: "Patient travelling next week, needs early session");

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Session.IntervalOverrideReason.Should().Be("Patient travelling next week, needs early session");
    }

    [Fact]
    public async Task RecordSession_FinalSession_AutoCompletesPackage()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage(totalSessions: 2, minIntervalDays: 0);

        // Record first session (1 of 2)
        package.RecordSession(
            "{\"energy\":15}", null, null, null,
            DefaultUserId, null, null, null, []);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var command = CreateRecordCommand(package.Id);

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert -- TRT-04: auto-completion
        result.IsSuccess.Should().BeTrue();
        result.Value.Session.SessionNumber.Should().Be(2);
        package.Status.Should().Be(PackageStatus.Completed);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordSession_OsdiScoreRecorded_StoredOnSession()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage();
        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var command = CreateRecordCommand(
            package.Id,
            osdiScore: 42.5m,
            osdiSeverity: "Severe");

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert -- TRT-03: OSDI score per session
        result.IsSuccess.Should().BeTrue();
        result.Value.Session.OsdiScore.Should().Be(42.5m);
        result.Value.Session.OsdiSeverity.Should().Be("Severe");
    }

    [Fact]
    public async Task RecordSession_WithConsumables_IncludesConsumablesInResponse()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage();
        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var consumableId = Guid.NewGuid();
        var consumables = new List<RecordTreatmentSessionCommand.ConsumableInput>
        {
            new(consumableId, "IPL Gel 50ml", 2),
            new(Guid.NewGuid(), "Eye Shield Disposable", 1)
        };

        var command = CreateRecordCommand(package.Id, consumables: consumables);

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert -- TRT-11: consumables tracked
        result.IsSuccess.Should().BeTrue();
        result.Value.Session.Consumables.Should().HaveCount(2);
        result.Value.Session.Consumables[0].ConsumableName.Should().Be("IPL Gel 50ml");
        result.Value.Session.Consumables[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task RecordSession_ValidationFails_ReturnsValidationError()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("PackageId", "Package ID is required.")
        };
        _validator.ValidateAsync(Arg.Any<RecordTreatmentSessionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var command = CreateRecordCommand(Guid.Empty);

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task RecordSession_DomainEventRaised_ContainsConsumablesForPharmacy()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage();
        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var consumableId = Guid.NewGuid();
        var consumables = new List<RecordTreatmentSessionCommand.ConsumableInput>
        {
            new(consumableId, "IPL Gel 50ml", 3)
        };

        var command = CreateRecordCommand(package.Id, consumables: consumables);

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert -- Domain event should contain consumable data for Pharmacy module
        result.IsSuccess.Should().BeTrue();
        package.DomainEvents.Should().ContainSingle(e =>
            e is Treatment.Domain.Events.TreatmentSessionCompletedEvent);

        var sessionEvent = package.DomainEvents
            .OfType<Treatment.Domain.Events.TreatmentSessionCompletedEvent>()
            .First();
        sessionEvent.Consumables.Should().HaveCount(1);
        sessionEvent.Consumables[0].ConsumableItemId.Should().Be(consumableId);
        sessionEvent.Consumables[0].Quantity.Should().Be(3);
    }

    [Fact]
    public async Task RecordSession_UsesCurrentUserIdAsPerformedById()
    {
        // Arrange
        SetupValidValidator();
        var package = CreateActivePackage();
        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        // Command specifies a different PerformedById than the current user
        var differentUserId = Guid.NewGuid();
        var command = new RecordTreatmentSessionCommand(
            PackageId: package.Id,
            ParametersJson: "{\"energy\":15}",
            OsdiScore: 25.5m,
            OsdiSeverity: "Moderate",
            ClinicalNotes: "Test",
            PerformedById: differentUserId,  // This should be ignored
            VisitId: null,
            ScheduledAt: null,
            IntervalOverrideReason: null,
            Consumables: []);

        // Act
        var result = await RecordTreatmentSessionHandler.Handle(
            command, _packageRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert -- Handler should use ICurrentUser.UserId, not command.PerformedById
        result.IsSuccess.Should().BeTrue();
        result.Value.Session.PerformedById.Should().Be(DefaultUserId);
        result.Value.Session.PerformedById.Should().NotBe(differentUserId);
    }

    // =========================================================================
    // GetTreatmentSessions Tests
    // =========================================================================

    [Fact]
    public async Task GetSessions_PackageWithSessions_ReturnsOrderedList()
    {
        // Arrange
        var package = CreateActivePackage(totalSessions: 4);
        package.RecordSession("{\"energy\":15}", 30m, "Moderate", "Good", DefaultUserId, null, null, null, []);
        package.RecordSession("{\"energy\":16}", 22m, "Mild", "Better", DefaultUserId, null, null, "Test setup", []);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var query = new GetTreatmentSessionsQuery(package.Id);

        // Act
        var result = await GetTreatmentSessionsHandler.Handle(
            query, _packageRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].SessionNumber.Should().Be(1);
        result.Value[1].SessionNumber.Should().Be(2);
        result.Value[0].OsdiScore.Should().Be(30m);
        result.Value[1].OsdiScore.Should().Be(22m);
    }

    [Fact]
    public async Task GetSessions_PackageWithNoSessions_ReturnsEmptyList()
    {
        // Arrange
        var package = CreateActivePackage();
        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var query = new GetTreatmentSessionsQuery(package.Id);

        // Act
        var result = await GetTreatmentSessionsHandler.Handle(
            query, _packageRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSessions_PackageNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        _packageRepository.GetByIdAsync(packageId, Arg.Any<CancellationToken>())
            .Returns((TreatmentPackage?)null);

        var query = new GetTreatmentSessionsQuery(packageId);

        // Act
        var result = await GetTreatmentSessionsHandler.Handle(
            query, _packageRepository, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    // =========================================================================
    // GetDueSoonSessions Tests
    // =========================================================================

    [Fact]
    public async Task GetDueSoon_ActivePackagesWithNoSessions_IncludedImmediately()
    {
        // Arrange
        var package = CreateActivePackage();
        _packageRepository.GetDueSoonAsync(Arg.Any<CancellationToken>())
            .Returns([package]);

        var query = new GetDueSoonSessionsQuery();

        // Act
        var result = await GetDueSoonSessionsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert -- packages with no sessions are immediately due
        result.Should().HaveCount(1);
        result[0].PatientName.Should().Be("Nguyen Van A");
        result[0].SessionsCompleted.Should().Be(0);
    }

    [Fact]
    public async Task GetDueSoon_NoActivePackages_ReturnsEmptyList()
    {
        // Arrange
        _packageRepository.GetDueSoonAsync(Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentPackage>());

        var query = new GetDueSoonSessionsQuery();

        // Act
        var result = await GetDueSoonSessionsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDueSoon_MapsNextDueDateCorrectly()
    {
        // Arrange
        var package = CreateActivePackage(totalSessions: 4, minIntervalDays: 14);
        // Record a session so we can compute next due date
        package.RecordSession("{\"energy\":15}", null, null, null, DefaultUserId, null, null, null, []);

        _packageRepository.GetDueSoonAsync(Arg.Any<CancellationToken>())
            .Returns([package]);

        var query = new GetDueSoonSessionsQuery();

        // Act
        var result = await GetDueSoonSessionsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].LastSessionDate.Should().NotBeNull();
        result[0].NextDueDate.Should().NotBeNull();
        result[0].SessionsCompleted.Should().Be(1);
        result[0].SessionsRemaining.Should().Be(3);
    }
}
