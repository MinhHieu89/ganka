using Billing.Application.Features;
using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Application;
using Shared.Domain;

namespace Billing.Unit.Tests.Features;

/// <summary>
/// TDD tests for Shift Management handlers: OpenShift, CloseShift, GetCurrentShift, GetShiftReport.
/// FIN-10: Enable cashier shift lifecycle with opening/closing, cash reconciliation, and shift reporting.
/// </summary>
public class ShiftHandlerTests
{
    private readonly ICashierShiftRepository _shiftRepository = Substitute.For<ICashierShiftRepository>();
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IValidator<OpenShiftCommand> _openShiftValidator = Substitute.For<IValidator<OpenShiftCommand>>();
    private readonly IValidator<CloseShiftCommand> _closeShiftValidator = Substitute.For<IValidator<CloseShiftCommand>>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultCashierId = Guid.NewGuid();
    private static readonly string DefaultCashierName = "Nguyễn Thị Hạnh";

    public ShiftHandlerTests()
    {
        _currentUser.UserId.Returns(DefaultCashierId);
        _currentUser.BranchId.Returns(DefaultBranchId);
        _currentUser.Email.Returns("hanh@ganka28.com");
    }

    private void SetupValidOpenShiftValidator()
    {
        _openShiftValidator.ValidateAsync(Arg.Any<OpenShiftCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidCloseShiftValidator()
    {
        _closeShiftValidator.ValidateAsync(Arg.Any<CloseShiftCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static CashierShift CreateOpenShift(decimal openingBalance = 1000000m)
    {
        return CashierShift.Create(
            cashierId: DefaultCashierId,
            cashierName: DefaultCashierName,
            openingBalance: openingBalance,
            shiftTemplateId: null,
            branchId: new BranchId(DefaultBranchId));
    }

    #region OpenShift Tests

    [Fact]
    public async Task OpenShift_NoExistingOpen_CreatesShift()
    {
        // Arrange
        SetupValidOpenShiftValidator();
        var command = new OpenShiftCommand(OpeningBalance: 500000m, ShiftTemplateId: null);

        _shiftRepository.GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns((CashierShift?)null);

        // Act
        var result = await OpenShiftHandler.Handle(
            command, _shiftRepository, _unitOfWork, _openShiftValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.OpeningBalance.Should().Be(500000m);
        result.Value.Status.Should().Be((int)ShiftStatus.Open);
        _shiftRepository.Received(1).Add(Arg.Any<CashierShift>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OpenShift_ExistingOpen_ReturnsError()
    {
        // Arrange
        SetupValidOpenShiftValidator();
        var command = new OpenShiftCommand(OpeningBalance: 500000m, ShiftTemplateId: null);
        var existingShift = CreateOpenShift();

        _shiftRepository.GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns(existingShift);

        // Act
        var result = await OpenShiftHandler.Handle(
            command, _shiftRepository, _unitOfWork, _openShiftValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already open");
        _shiftRepository.DidNotReceive().Add(Arg.Any<CashierShift>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region CloseShift Tests

    [Fact]
    public async Task CloseShift_OpenShift_LocksAndCloses()
    {
        // Arrange
        SetupValidCloseShiftValidator();
        var command = new CloseShiftCommand(ActualCashCount: 500000m, ManagerNote: "Ca sáng kết thúc tốt");
        var shift = CreateOpenShift(openingBalance: 500000m);

        _shiftRepository.GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns(shift);

        // Act
        var result = await CloseShiftHandler.Handle(
            command, _shiftRepository, _unitOfWork, _closeShiftValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be((int)ShiftStatus.Closed);
        result.Value.ActualCashCount.Should().Be(500000m);
        result.Value.Discrepancy.Should().Be(0m); // 500000 actual - 500000 expected = 0
        result.Value.ManagerNote.Should().Be("Ca sáng kết thúc tốt");
        _shiftRepository.Received(1).Update(Arg.Any<CashierShift>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CloseShift_NoOpenShift_ReturnsError()
    {
        // Arrange
        SetupValidCloseShiftValidator();
        var command = new CloseShiftCommand(ActualCashCount: 500000m, ManagerNote: null);

        _shiftRepository.GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns((CashierShift?)null);

        // Act
        var result = await CloseShiftHandler.Handle(
            command, _shiftRepository, _unitOfWork, _closeShiftValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("No open shift");
        _shiftRepository.DidNotReceive().Update(Arg.Any<CashierShift>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetCurrentShift Tests

    [Fact]
    public async Task GetCurrentShift_HasOpenShift_ReturnsShiftDto()
    {
        // Arrange
        var query = new GetCurrentShiftQuery();
        var shift = CreateOpenShift(openingBalance: 1000000m);

        _shiftRepository.GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns(shift);

        // Act
        var result = await GetCurrentShiftHandler.Handle(
            query, _shiftRepository, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.OpeningBalance.Should().Be(1000000m);
        result.Value.Status.Should().Be((int)ShiftStatus.Open);
        result.Value.CashierName.Should().Be(DefaultCashierName);
    }

    [Fact]
    public async Task GetCurrentShift_NoOpenShift_ReturnsNull()
    {
        // Arrange
        var query = new GetCurrentShiftQuery();

        _shiftRepository.GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns((CashierShift?)null);

        // Act
        var result = await GetCurrentShiftHandler.Handle(
            query, _shiftRepository, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    #endregion

    #region GetShiftReport Tests

    [Fact]
    public async Task GetShiftReport_ClosedShift_ReturnsRevenueByMethod()
    {
        // Arrange
        var shift = CreateOpenShift(openingBalance: 200000m);
        // Simulate cash received on the shift
        shift.AddCashReceived(300000m);
        shift.AddNonCashRevenue(150000m);
        shift.IncrementTransactionCount();
        shift.IncrementTransactionCount();
        shift.IncrementTransactionCount();
        // Lock and close
        shift.LockForClose();
        shift.Close(actualCashCount: 490000m, managerNote: "Thiếu 10k");

        var shiftId = shift.Id;
        var query = new GetShiftReportQuery(ShiftId: shiftId);

        _shiftRepository.GetByIdAsync(shiftId, Arg.Any<CancellationToken>())
            .Returns(shift);

        // Mock payments for revenue-by-method grouping
        var payments = new List<Payment>
        {
            CreateTestPayment(PaymentMethod.Cash, 200000m),
            CreateTestPayment(PaymentMethod.Cash, 100000m),
            CreateTestPayment(PaymentMethod.QrMomo, 150000m)
        };

        _paymentRepository.GetByShiftIdAsync(shiftId, Arg.Any<CancellationToken>())
            .Returns(payments);

        // Act
        var result = await GetShiftReportHandler.Handle(
            query, _shiftRepository, _paymentRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var report = result.Value;
        report.ShiftId.Should().Be(shiftId);
        report.CashierName.Should().Be(DefaultCashierName);
        report.TransactionCount.Should().Be(3);
        report.OpeningBalance.Should().Be(200000m);
        report.CashReceived.Should().Be(300000m);
        report.ActualCash.Should().Be(490000m);
        report.Discrepancy.Should().Be(-10000m); // 490000 - (200000 + 300000 - 0) = -10000
        report.ManagerNote.Should().Be("Thiếu 10k");

        // Revenue breakdown by payment method
        report.RevenueByMethod.Should().ContainKey("Cash");
        report.RevenueByMethod["Cash"].Should().Be(300000m);
        report.RevenueByMethod.Should().ContainKey("QrMomo");
        report.RevenueByMethod["QrMomo"].Should().Be(150000m);
    }

    #endregion

    #region Helpers

    private static Payment CreateTestPayment(PaymentMethod method, decimal amount)
    {
        return Payment.Create(
            invoiceId: Guid.NewGuid(),
            method: method,
            amount: amount,
            recordedById: DefaultCashierId,
            cashierShiftId: null);
    }

    #endregion
}
