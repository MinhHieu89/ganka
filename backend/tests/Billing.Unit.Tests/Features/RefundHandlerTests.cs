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
using Wolverine;

namespace Billing.Unit.Tests.Features;

public class RefundHandlerTests
{
    private static readonly BranchId DefaultBranchId =
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<RequestRefundCommand> _validator = Substitute.For<IValidator<RequestRefundCommand>>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly ICashierShiftRepository _cashierShiftRepository = Substitute.For<ICashierShiftRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public RefundHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.BranchId.Returns(DefaultBranchId.Value);
    }

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<RequestRefundCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private Invoice CreateDraftInvoice(decimal unitPrice = 500_000m, int quantity = 1)
    {
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test Patient", Guid.NewGuid(), DefaultBranchId);
        invoice.AddLineItem("Eye Exam", "Kham mat", unitPrice, quantity, Department.Medical);
        return invoice;
    }

    private Invoice CreateFinalizedInvoice(decimal unitPrice = 500_000m, int quantity = 1)
    {
        var invoice = CreateDraftInvoice(unitPrice, quantity);
        var payment = Payment.Create(invoice.Id, PaymentMethod.Cash, invoice.TotalAmount, Guid.NewGuid());
        payment.Confirm();
        invoice.RecordPayment(payment);
        invoice.Finalize(Guid.NewGuid(), Guid.NewGuid());
        return invoice;
    }

    private Invoice CreateFinalizedInvoiceWithRefund(out Refund refund, RefundStatus targetStatus = RefundStatus.Requested)
    {
        var invoice = CreateFinalizedInvoice(500_000m, 1);
        refund = Refund.Create(invoice.Id, 100_000m, "Customer complaint", Guid.NewGuid());
        invoice.AddRefund(refund);

        if (targetStatus >= RefundStatus.Approved)
            refund.Approve(Guid.NewGuid());

        return invoice;
    }

    [Fact]
    public async Task RequestRefund_FinalizedInvoice_CreatesRequestedRefund()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateFinalizedInvoice(500_000m, 1); // TotalAmount = 500,000
        var command = new RequestRefundCommand(
            invoice.Id, null, 100_000m, "Customer complaint");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RequestRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100_000m);
        result.Value.Status.Should().Be((int)RefundStatus.Requested);
        result.Value.Reason.Should().Be("Customer complaint");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestRefund_DraftInvoice_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(500_000m, 1);
        var command = new RequestRefundCommand(
            invoice.Id, null, 100_000m, "Refund attempt on draft");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RequestRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task RequestRefund_ExceedsTotal_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateFinalizedInvoice(500_000m, 1); // TotalAmount = 500,000
        var command = new RequestRefundCommand(
            invoice.Id, null, 999_999m, "Excessive refund");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RequestRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _validator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // ===== ApproveRefund Tests =====

    [Fact]
    public async Task ApproveRefund_ValidPin_ApprovesRefund()
    {
        // Arrange
        var invoice = CreateFinalizedInvoiceWithRefund(out var refund);
        var managerId = Guid.NewGuid();
        var command = new ApproveRefundCommand(invoice.Id, refund.Id, managerId, "1234");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        _messageBus.InvokeAsync<VerifyManagerPinResponse>(
            Arg.Any<VerifyManagerPinQuery>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyManagerPinResponse(true));

        // Act
        var result = await ApproveRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refund.Status.Should().Be(RefundStatus.Approved);
        refund.ApprovedById.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveRefund_InvalidPin_ReturnsError()
    {
        // Arrange
        var invoice = CreateFinalizedInvoiceWithRefund(out var refund);
        var command = new ApproveRefundCommand(invoice.Id, refund.Id, Guid.NewGuid(), "wrong-pin");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        _messageBus.InvokeAsync<VerifyManagerPinResponse>(
            Arg.Any<VerifyManagerPinQuery>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyManagerPinResponse(false));

        // Act
        var result = await ApproveRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Invalid manager PIN");
    }

    [Fact]
    public async Task ApproveRefund_NotRequested_ReturnsError()
    {
        // Arrange - refund is already Approved
        var invoice = CreateFinalizedInvoiceWithRefund(out var refund, RefundStatus.Approved);
        var command = new ApproveRefundCommand(invoice.Id, refund.Id, Guid.NewGuid(), "1234");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApproveRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Requested");
    }

    [Fact]
    public async Task ApproveRefund_RefundNotFound_ReturnsError()
    {
        // Arrange
        var invoice = CreateFinalizedInvoice();
        var command = new ApproveRefundCommand(invoice.Id, Guid.NewGuid(), Guid.NewGuid(), "1234");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApproveRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    // ===== ProcessRefund Tests =====

    [Fact]
    public async Task ProcessRefund_CashRefund_UpdatesShiftCashRefunds()
    {
        // Arrange
        var invoice = CreateFinalizedInvoiceWithRefund(out var refund, RefundStatus.Approved);
        var shift = CashierShift.Create(
            Guid.NewGuid(), "Test Cashier", 1_000_000m, null, DefaultBranchId);
        var command = new ProcessRefundCommand(
            invoice.Id, refund.Id, (int)PaymentMethod.Cash, "Refunding via cash");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        _cashierShiftRepository.GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns(shift);

        // Act
        var result = await ProcessRefundHandler.Handle(
            command, _invoiceRepository, _cashierShiftRepository,
            _paymentRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refund.Status.Should().Be(RefundStatus.Processed);
        refund.ProcessedById.Should().NotBeNull();
        shift.CashRefunds.Should().Be(100_000m);
        result.Value.Status.Should().Be((int)RefundStatus.Processed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRefund_NonCashRefund_DoesNotUpdateShift()
    {
        // Arrange
        var invoice = CreateFinalizedInvoiceWithRefund(out var refund, RefundStatus.Approved);
        var command = new ProcessRefundCommand(
            invoice.Id, refund.Id, (int)PaymentMethod.BankTransfer, "Bank transfer refund");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ProcessRefundHandler.Handle(
            command, _invoiceRepository, _cashierShiftRepository,
            _paymentRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refund.Status.Should().Be(RefundStatus.Processed);
        // No shift should be loaded for non-cash refunds
        await _cashierShiftRepository.DidNotReceive()
            .GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRefund_NotApproved_ReturnsError()
    {
        // Arrange - refund is still in Requested status
        var invoice = CreateFinalizedInvoiceWithRefund(out var refund);
        var command = new ProcessRefundCommand(
            invoice.Id, refund.Id, (int)PaymentMethod.Cash, null);

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ProcessRefundHandler.Handle(
            command, _invoiceRepository, _cashierShiftRepository,
            _paymentRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("approved");
    }

    [Fact]
    public async Task ProcessRefund_CashRefund_NoOpenShift_ReturnsError()
    {
        // Arrange
        var invoice = CreateFinalizedInvoiceWithRefund(out var refund, RefundStatus.Approved);
        var command = new ProcessRefundCommand(
            invoice.Id, refund.Id, (int)PaymentMethod.Cash, null);

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        _cashierShiftRepository.GetCurrentOpenAsync(Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns((CashierShift?)null);

        // Act
        var result = await ProcessRefundHandler.Handle(
            command, _invoiceRepository, _cashierShiftRepository,
            _paymentRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("shift");
    }
}
