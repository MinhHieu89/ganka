using FluentValidation;
using FluentValidation.Results;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Features;
using Treatment.Contracts.Dtos;

namespace Treatment.Unit.Tests.Features;

public class TreatmentPackageHandlerTests
{
    private readonly ITreatmentProtocolRepository _protocolRepository = Substitute.For<ITreatmentProtocolRepository>();
    private readonly ITreatmentPackageRepository _packageRepository = Substitute.For<ITreatmentPackageRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private readonly IValidator<CreateTreatmentPackageCommand> _createValidator =
        Substitute.For<IValidator<CreateTreatmentPackageCommand>>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public TreatmentPackageHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
        _currentUser.UserId.Returns(DefaultUserId);
    }

    private void SetupValidCreateValidator()
    {
        _createValidator
            .ValidateAsync(Arg.Any<CreateTreatmentPackageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private TreatmentProtocol CreateTestProtocol()
    {
        return TreatmentProtocol.Create(
            name: "Standard IPL 4-session",
            treatmentType: TreatmentType.IPL,
            defaultSessionCount: 4,
            pricingMode: PricingMode.PerPackage,
            defaultPackagePrice: 5000000m,
            defaultSessionPrice: 1500000m,
            minIntervalDays: 14,
            maxIntervalDays: 28,
            defaultParametersJson: """{"energy":12,"pulseCount":5}""",
            cancellationDeductionPercent: 15m,
            description: "Standard IPL treatment for dry eye",
            branchId: new BranchId(DefaultBranchId));
    }

    private TreatmentPackage CreateTestPackage(TreatmentProtocol? protocol = null)
    {
        var p = protocol ?? CreateTestProtocol();
        return TreatmentPackage.Create(
            protocolTemplateId: p.Id,
            patientId: Guid.NewGuid(),
            patientName: "Nguyen Van A",
            treatmentType: p.TreatmentType,
            totalSessions: p.DefaultSessionCount,
            pricingMode: p.PricingMode,
            packagePrice: p.DefaultPackagePrice,
            sessionPrice: p.DefaultSessionPrice,
            minIntervalDays: p.MinIntervalDays,
            parametersJson: p.DefaultParametersJson ?? "{}",
            visitId: null,
            createdById: DefaultUserId,
            branchId: new BranchId(DefaultBranchId));
    }

    #region CreateTreatmentPackage Tests

    [Fact]
    public async Task CreateTreatmentPackage_ValidCommand_CreatesPackageWithTemplateDefaults()
    {
        // Arrange
        SetupValidCreateValidator();
        var protocol = CreateTestProtocol();
        var patientId = Guid.NewGuid();

        var command = new CreateTreatmentPackageCommand(
            ProtocolTemplateId: protocol.Id,
            PatientId: patientId,
            PatientName: "Nguyen Van A",
            TotalSessions: null,
            PricingMode: null,
            PackagePrice: null,
            SessionPrice: null,
            MinIntervalDays: null,
            ParametersJson: null,
            VisitId: null);

        _protocolRepository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>())
            .Returns(protocol);

        // Act
        var result = await CreateTreatmentPackageHandler.Handle(
            command, _protocolRepository, _packageRepository, _unitOfWork,
            _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PatientId.Should().Be(patientId);
        result.Value.PatientName.Should().Be("Nguyen Van A");
        result.Value.TreatmentType.Should().Be(nameof(TreatmentType.IPL));
        result.Value.Status.Should().Be(nameof(PackageStatus.Active));
        result.Value.TotalSessions.Should().Be(4);
        result.Value.SessionsCompleted.Should().Be(0);
        result.Value.SessionsRemaining.Should().Be(4);
        result.Value.PricingMode.Should().Be(nameof(PricingMode.PerPackage));
        result.Value.PackagePrice.Should().Be(5000000m);
        result.Value.SessionPrice.Should().Be(1500000m);
        result.Value.MinIntervalDays.Should().Be(14);
        result.Value.Sessions.Should().BeEmpty();
        _packageRepository.Received(1).Add(Arg.Any<TreatmentPackage>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTreatmentPackage_NonExistentTemplate_ReturnsNotFound()
    {
        // Arrange
        SetupValidCreateValidator();
        var nonExistentId = Guid.NewGuid();
        var command = new CreateTreatmentPackageCommand(
            ProtocolTemplateId: nonExistentId,
            PatientId: Guid.NewGuid(),
            PatientName: "Test Patient",
            TotalSessions: null,
            PricingMode: null,
            PackagePrice: null,
            SessionPrice: null,
            MinIntervalDays: null,
            ParametersJson: null,
            VisitId: null);

        _protocolRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((TreatmentProtocol?)null);

        // Act
        var result = await CreateTreatmentPackageHandler.Handle(
            command, _protocolRepository, _packageRepository, _unitOfWork,
            _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task CreateTreatmentPackage_CustomOverrides_UsesOverridesInsteadOfDefaults()
    {
        // Arrange
        SetupValidCreateValidator();
        var protocol = CreateTestProtocol();
        var patientId = Guid.NewGuid();

        var command = new CreateTreatmentPackageCommand(
            ProtocolTemplateId: protocol.Id,
            PatientId: patientId,
            PatientName: "Tran Thi B",
            TotalSessions: 6,
            PricingMode: (int)PricingMode.PerSession,
            PackagePrice: 0m,
            SessionPrice: 2000000m,
            MinIntervalDays: 7,
            ParametersJson: """{"energy":15,"pulseCount":8}""",
            VisitId: Guid.NewGuid());

        _protocolRepository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>())
            .Returns(protocol);

        // Act
        var result = await CreateTreatmentPackageHandler.Handle(
            command, _protocolRepository, _packageRepository, _unitOfWork,
            _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalSessions.Should().Be(6);
        result.Value.PricingMode.Should().Be(nameof(PricingMode.PerSession));
        result.Value.SessionPrice.Should().Be(2000000m);
        result.Value.MinIntervalDays.Should().Be(7);
        result.Value.ParametersJson.Should().Be("""{"energy":15,"pulseCount":8}""");
    }

    [Fact]
    public async Task CreateTreatmentPackage_ValidationFailure_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateTreatmentPackageCommand(
            ProtocolTemplateId: Guid.Empty,
            PatientId: Guid.Empty,
            PatientName: "",
            TotalSessions: null,
            PricingMode: null,
            PackagePrice: null,
            SessionPrice: null,
            MinIntervalDays: null,
            ParametersJson: null,
            VisitId: null);

        _createValidator
            .ValidateAsync(Arg.Any<CreateTreatmentPackageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("ProtocolTemplateId", "Protocol Template ID is required."),
                new ValidationFailure("PatientId", "Patient ID is required.")
            }));

        // Act
        var result = await CreateTreatmentPackageHandler.Handle(
            command, _protocolRepository, _packageRepository, _unitOfWork,
            _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    #endregion

    #region GetPatientTreatments Tests

    [Fact]
    public async Task GetPatientTreatments_PatientWithPackages_ReturnsListWithSessionCounts()
    {
        // Arrange
        var protocol = CreateTestProtocol();
        var patientId = Guid.NewGuid();
        var package1 = TreatmentPackage.Create(
            protocolTemplateId: protocol.Id,
            patientId: patientId,
            patientName: "Nguyen Van A",
            treatmentType: TreatmentType.IPL,
            totalSessions: 4,
            pricingMode: PricingMode.PerPackage,
            packagePrice: 5000000m,
            sessionPrice: 1500000m,
            minIntervalDays: 14,
            parametersJson: "{}",
            visitId: null,
            createdById: DefaultUserId,
            branchId: new BranchId(DefaultBranchId));

        _packageRepository.GetByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentPackage> { package1 });

        _protocolRepository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>())
            .Returns(protocol);

        var query = new GetPatientTreatmentsQuery(patientId);

        // Act
        var result = await GetPatientTreatmentsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PatientId.Should().Be(patientId);
        result.Value[0].SessionsCompleted.Should().Be(0);
        result.Value[0].SessionsRemaining.Should().Be(4);
    }

    [Fact]
    public async Task GetPatientTreatments_PatientWithNoPackages_ReturnsEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _packageRepository.GetByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentPackage>());

        var query = new GetPatientTreatmentsQuery(patientId);

        // Act
        var result = await GetPatientTreatmentsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPatientTreatments_MultipleActivePackages_ReturnsAll()
    {
        // Arrange
        var protocol = CreateTestProtocol();
        var patientId = Guid.NewGuid();

        var package1 = TreatmentPackage.Create(
            protocolTemplateId: protocol.Id,
            patientId: patientId,
            patientName: "Nguyen Van A",
            treatmentType: TreatmentType.IPL,
            totalSessions: 4,
            pricingMode: PricingMode.PerPackage,
            packagePrice: 5000000m,
            sessionPrice: 1500000m,
            minIntervalDays: 14,
            parametersJson: "{}",
            visitId: null,
            createdById: DefaultUserId,
            branchId: new BranchId(DefaultBranchId));

        var package2 = TreatmentPackage.Create(
            protocolTemplateId: protocol.Id,
            patientId: patientId,
            patientName: "Nguyen Van A",
            treatmentType: TreatmentType.LLLT,
            totalSessions: 6,
            pricingMode: PricingMode.PerSession,
            packagePrice: 0m,
            sessionPrice: 800000m,
            minIntervalDays: 7,
            parametersJson: "{}",
            visitId: null,
            createdById: DefaultUserId,
            branchId: new BranchId(DefaultBranchId));

        _packageRepository.GetByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentPackage> { package1, package2 });

        _protocolRepository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>())
            .Returns(protocol);

        var query = new GetPatientTreatmentsQuery(patientId);

        // Act
        var result = await GetPatientTreatmentsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    #endregion

    #region GetTreatmentPackageById Tests

    [Fact]
    public async Task GetTreatmentPackageById_ExistingId_ReturnsFullPackageDto()
    {
        // Arrange
        var protocol = CreateTestProtocol();
        var package = CreateTestPackage(protocol);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        _protocolRepository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>())
            .Returns(protocol);

        var query = new GetTreatmentPackageByIdQuery(package.Id);

        // Act
        var result = await GetTreatmentPackageByIdHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(package.Id);
        result.Value.ProtocolTemplateName.Should().Be("Standard IPL 4-session");
        result.Value.TreatmentType.Should().Be(nameof(TreatmentType.IPL));
        result.Value.Sessions.Should().BeEmpty();
        result.Value.CancellationRequest.Should().BeNull();
    }

    [Fact]
    public async Task GetTreatmentPackageById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _packageRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((TreatmentPackage?)null);

        var query = new GetTreatmentPackageByIdQuery(nonExistentId);

        // Act
        var result = await GetTreatmentPackageByIdHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    #endregion

    #region GetActiveTreatments Tests

    [Fact]
    public async Task GetActiveTreatments_ActivePackagesExist_ReturnsAll()
    {
        // Arrange
        var protocol = CreateTestProtocol();
        var package1 = CreateTestPackage(protocol);
        var package2 = CreateTestPackage(protocol);

        _packageRepository.GetActivePackagesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentPackage> { package1, package2 });

        _protocolRepository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>())
            .Returns(protocol);

        var query = new GetActiveTreatmentsQuery();

        // Act
        var result = await GetActiveTreatmentsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveTreatments_NoActivePackages_ReturnsEmptyList()
    {
        // Arrange
        _packageRepository.GetActivePackagesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentPackage>());

        var query = new GetActiveTreatmentsQuery();

        // Act
        var result = await GetActiveTreatmentsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region Cross-Module GetPatientTreatmentsQuery (Contracts) Tests

    [Fact]
    public async Task GetPatientTreatmentsContractsQuery_ReturnsPackageDtos()
    {
        // Arrange
        var protocol = CreateTestProtocol();
        var patientId = Guid.NewGuid();
        var package = TreatmentPackage.Create(
            protocolTemplateId: protocol.Id,
            patientId: patientId,
            patientName: "Le Van C",
            treatmentType: TreatmentType.LidCare,
            totalSessions: 3,
            pricingMode: PricingMode.PerSession,
            packagePrice: 0m,
            sessionPrice: 600000m,
            minIntervalDays: 7,
            parametersJson: "{}",
            visitId: null,
            createdById: DefaultUserId,
            branchId: new BranchId(DefaultBranchId));

        _packageRepository.GetByPatientIdAsync(patientId, Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentPackage> { package });

        _protocolRepository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>())
            .Returns(protocol);

        var query = new Treatment.Contracts.Queries.GetPatientTreatmentsQuery(patientId);

        // Act
        var result = await GetPatientTreatmentsHandler.HandleCrossModule(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].PatientName.Should().Be("Le Van C");
        result[0].TreatmentType.Should().Be(nameof(TreatmentType.LidCare));
    }

    #endregion
}
