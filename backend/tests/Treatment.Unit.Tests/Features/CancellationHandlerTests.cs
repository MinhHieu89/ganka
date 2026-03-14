using Auth.Contracts.Queries;
using FluentValidation;
using FluentValidation.Results;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Features;
using Wolverine;

namespace Treatment.Unit.Tests.Features;

public class CancellationHandlerTests
{
    private static readonly BranchId DefaultBranchId =
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private readonly ITreatmentPackageRepository _packageRepository = Substitute.For<ITreatmentPackageRepository>();
    private readonly ITreatmentProtocolRepository _protocolRepository = Substitute.For<ITreatmentProtocolRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IValidator<RequestCancellationCommand> _requestValidator = Substitute.For<IValidator<RequestCancellationCommand>>();
    private readonly IValidator<ApproveCancellationCommand> _approveValidator = Substitute.For<IValidator<ApproveCancellationCommand>>();
    private readonly IValidator<RejectCancellationCommand> _rejectValidator = Substitute.For<IValidator<RejectCancellationCommand>>();

    public CancellationHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.BranchId.Returns(DefaultBranchId.Value);
    }

    // --- Helper methods ---

    private TreatmentProtocol CreateProtocol(decimal cancellationDeductionPercent = 15m)
    {
        return TreatmentProtocol.Create(
            name: "Standard IPL 4-session",
            treatmentType: TreatmentType.IPL,
            defaultSessionCount: 4,
            pricingMode: PricingMode.PerSession,
            defaultPackagePrice: 2_000_000m,
            defaultSessionPrice: 500_000m,
            minIntervalDays: 14,
            maxIntervalDays: 28,
            defaultParametersJson: "{}",
            cancellationDeductionPercent: cancellationDeductionPercent,
            description: "Test protocol",
            branchId: DefaultBranchId);
    }

    private TreatmentPackage CreateActivePackage(
        PricingMode pricingMode = PricingMode.PerSession,
        int totalSessions = 4,
        decimal packagePrice = 2_000_000m,
        decimal sessionPrice = 500_000m,
        Guid? protocolId = null)
    {
        var protocol = CreateProtocol();
        return TreatmentPackage.Create(
            protocolTemplateId: protocolId ?? protocol.Id,
            patientId: Guid.NewGuid(),
            patientName: "Test Patient",
            treatmentType: TreatmentType.IPL,
            totalSessions: totalSessions,
            pricingMode: pricingMode,
            packagePrice: packagePrice,
            sessionPrice: sessionPrice,
            minIntervalDays: 14,
            parametersJson: "{}",
            visitId: null,
            createdById: Guid.NewGuid(),
            branchId: DefaultBranchId);
    }

    private TreatmentPackage CreatePendingCancellationPackage(
        PricingMode pricingMode = PricingMode.PerSession,
        int totalSessions = 4,
        decimal packagePrice = 2_000_000m,
        decimal sessionPrice = 500_000m,
        decimal deductionPercent = 15m)
    {
        var package = CreateActivePackage(pricingMode, totalSessions, packagePrice, sessionPrice);
        package.RequestCancellation("Patient wants to cancel", deductionPercent, Guid.NewGuid());
        return package;
    }

    private void SetupValidRequestValidator()
    {
        _requestValidator.ValidateAsync(Arg.Any<RequestCancellationCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidApproveValidator()
    {
        _approveValidator.ValidateAsync(Arg.Any<ApproveCancellationCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidRejectValidator()
    {
        _rejectValidator.ValidateAsync(Arg.Any<RejectCancellationCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    // ===== RequestCancellation Tests =====

    [Fact]
    public async Task RequestCancellation_ActivePackage_SetsPendingCancellation()
    {
        // Arrange
        SetupValidRequestValidator();
        var protocol = CreateProtocol(cancellationDeductionPercent: 15m);
        var package = CreateActivePackage(protocolId: protocol.Id);
        var command = new RequestCancellationCommand(package.Id, "Patient wants to cancel");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);
        _protocolRepository.GetByIdAsync(protocol.Id, Arg.Any<CancellationToken>()).Returns(protocol);

        // Act
        var result = await RequestCancellationHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _requestValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        package.Status.Should().Be(PackageStatus.PendingCancellation);
        package.CancellationRequest.Should().NotBeNull();
        package.CancellationRequest!.Reason.Should().Be("Patient wants to cancel");
        package.CancellationRequest.DeductionPercent.Should().Be(15m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestCancellation_PackageNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidRequestValidator();
        var packageId = Guid.NewGuid();
        var command = new RequestCancellationCommand(packageId, "Cancel reason");

        _packageRepository.GetByIdAsync(packageId, Arg.Any<CancellationToken>()).Returns((TreatmentPackage?)null);

        // Act
        var result = await RequestCancellationHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _requestValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task RequestCancellation_AlreadyPendingCancellation_ReturnsError()
    {
        // Arrange
        SetupValidRequestValidator();
        var protocol = CreateProtocol();
        var package = CreatePendingCancellationPackage();

        var command = new RequestCancellationCommand(package.Id, "Another cancel attempt");
        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await RequestCancellationHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _requestValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task RequestCancellation_ValidationFails_ReturnsValidationError()
    {
        // Arrange
        var command = new RequestCancellationCommand(Guid.NewGuid(), "");
        var failures = new List<ValidationFailure> { new("Reason", "Reason is required.") };
        _requestValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        // Act
        var result = await RequestCancellationHandler.Handle(
            command, _packageRepository, _protocolRepository, _unitOfWork, _requestValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // ===== ApproveCancellation Tests =====

    [Fact]
    public async Task ApproveCancellation_ValidPin_PerSession_ApprovesAndCalculatesRefund()
    {
        // Arrange
        SetupValidApproveValidator();
        // 4 sessions at 500k each, 0 completed, 15% deduction
        var package = CreatePendingCancellationPackage(
            pricingMode: PricingMode.PerSession,
            totalSessions: 4,
            sessionPrice: 500_000m,
            deductionPercent: 15m);

        var managerId = Guid.NewGuid();
        var command = new ApproveCancellationCommand(package.Id, managerId, "1234", 15m);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);
        _messageBus.InvokeAsync<VerifyManagerPinResponse>(
            Arg.Any<VerifyManagerPinQuery>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyManagerPinResponse(true));

        // Act
        var result = await ApproveCancellationHandler.Handle(
            command, _packageRepository, _unitOfWork, _messageBus, _approveValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        package.Status.Should().Be(PackageStatus.Cancelled);
        package.CancellationRequest.Should().NotBeNull();
        package.CancellationRequest!.Status.Should().Be(CancellationRequestStatus.Approved);
        // Refund = 4 remaining * 500k * (1 - 0.15) = 1,700,000
        package.CancellationRequest.RefundAmount.Should().Be(1_700_000m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveCancellation_ValidPin_PerPackage_ApprovesAndCalculatesRefund()
    {
        // Arrange
        SetupValidApproveValidator();
        // Package price 2M, 4 sessions, 0 completed, 20% deduction
        var package = CreatePendingCancellationPackage(
            pricingMode: PricingMode.PerPackage,
            totalSessions: 4,
            packagePrice: 2_000_000m,
            deductionPercent: 20m);

        var managerId = Guid.NewGuid();
        var command = new ApproveCancellationCommand(package.Id, managerId, "1234", 20m);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);
        _messageBus.InvokeAsync<VerifyManagerPinResponse>(
            Arg.Any<VerifyManagerPinQuery>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyManagerPinResponse(true));

        // Act
        var result = await ApproveCancellationHandler.Handle(
            command, _packageRepository, _unitOfWork, _messageBus, _approveValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        package.Status.Should().Be(PackageStatus.Cancelled);
        // Refund = 2M * 4/4 * (1 - 0.20) = 1,600,000
        package.CancellationRequest!.RefundAmount.Should().Be(1_600_000m);
    }

    [Fact]
    public async Task ApproveCancellation_InvalidPin_ReturnsError()
    {
        // Arrange
        SetupValidApproveValidator();
        var package = CreatePendingCancellationPackage();
        var command = new ApproveCancellationCommand(package.Id, Guid.NewGuid(), "wrong-pin", 15m);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);
        _messageBus.InvokeAsync<VerifyManagerPinResponse>(
            Arg.Any<VerifyManagerPinQuery>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyManagerPinResponse(false));

        // Act
        var result = await ApproveCancellationHandler.Handle(
            command, _packageRepository, _unitOfWork, _messageBus, _approveValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Invalid manager PIN");
    }

    [Fact]
    public async Task ApproveCancellation_DeductionOutsideRange_ReturnsValidationError()
    {
        // Arrange
        var command = new ApproveCancellationCommand(Guid.NewGuid(), Guid.NewGuid(), "1234", 5m);
        var failures = new List<ValidationFailure> { new("DeductionPercent", "Deduction percent must be between 10 and 20.") };
        _approveValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        // Act
        var result = await ApproveCancellationHandler.Handle(
            command, _packageRepository, _unitOfWork, _messageBus, _approveValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task ApproveCancellation_NotPending_ReturnsError()
    {
        // Arrange
        SetupValidApproveValidator();
        var package = CreateActivePackage(); // Active, not PendingCancellation
        var command = new ApproveCancellationCommand(package.Id, Guid.NewGuid(), "1234", 15m);

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await ApproveCancellationHandler.Handle(
            command, _packageRepository, _unitOfWork, _messageBus, _approveValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // ===== RejectCancellation Tests =====

    [Fact]
    public async Task RejectCancellation_PendingPackage_RejectsAndResetsToActive()
    {
        // Arrange
        SetupValidRejectValidator();
        var package = CreatePendingCancellationPackage();
        var managerId = Guid.NewGuid();
        var command = new RejectCancellationCommand(package.Id, managerId, "Treatment should continue");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await RejectCancellationHandler.Handle(
            command, _packageRepository, _unitOfWork, _rejectValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        package.Status.Should().Be(PackageStatus.Active);
        package.CancellationRequest.Should().NotBeNull();
        package.CancellationRequest!.Status.Should().Be(CancellationRequestStatus.Rejected);
        package.CancellationRequest.ProcessedById.Should().Be(managerId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectCancellation_NotPending_ReturnsError()
    {
        // Arrange
        SetupValidRejectValidator();
        var package = CreateActivePackage(); // Active, not PendingCancellation
        var command = new RejectCancellationCommand(package.Id, Guid.NewGuid(), "Reject reason");

        _packageRepository.GetByIdAsync(package.Id, Arg.Any<CancellationToken>()).Returns(package);

        // Act
        var result = await RejectCancellationHandler.Handle(
            command, _packageRepository, _unitOfWork, _rejectValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task RejectCancellation_ValidationFails_ReturnsValidationError()
    {
        // Arrange
        var command = new RejectCancellationCommand(Guid.NewGuid(), Guid.NewGuid(), "");
        var failures = new List<ValidationFailure> { new("RejectionReason", "Rejection reason is required.") };
        _rejectValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        // Act
        var result = await RejectCancellationHandler.Handle(
            command, _packageRepository, _unitOfWork, _rejectValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // ===== GetPendingCancellations Tests =====

    [Fact]
    public async Task GetPendingCancellations_ReturnsPendingPackages()
    {
        // Arrange
        var package1 = CreatePendingCancellationPackage();
        var package2 = CreatePendingCancellationPackage();
        var pendingPackages = new List<TreatmentPackage> { package1, package2 };

        _packageRepository.GetPendingCancellationsAsync(Arg.Any<CancellationToken>()).Returns(pendingPackages);

        var query = new GetPendingCancellationsQuery();

        // Act
        var result = await GetPendingCancellationsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(dto =>
        {
            dto.Status.Should().Be("PendingCancellation");
            dto.CancellationRequest.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task GetPendingCancellations_NoPending_ReturnsEmptyList()
    {
        // Arrange
        _packageRepository.GetPendingCancellationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<TreatmentPackage>());

        var query = new GetPendingCancellationsQuery();

        // Act
        var result = await GetPendingCancellationsHandler.Handle(
            query, _packageRepository, _protocolRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
