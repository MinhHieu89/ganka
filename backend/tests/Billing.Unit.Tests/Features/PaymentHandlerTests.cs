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

public class PaymentHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly ICashierShiftRepository _cashierShiftRepository = Substitute.For<ICashierShiftRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IValidator<RecordPaymentCommand> _validator = Substitute.For<IValidator<RecordPaymentCommand>>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public PaymentHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
        _currentUser.UserId.Returns(DefaultUserId);
    }

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<RecordPaymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private Invoice CreateDraftInvoice(decimal totalAmount = 500_000m)
    {
        var invoice = Invoice.Create(
            "HD-2026-00001",
            Guid.NewGuid(),
            "Nguyen Van A",
            Guid.NewGuid(),
            new BranchId(DefaultBranchId));

        // Add a line item to set the TotalAmount
        invoice.AddLineItem(
            "Examination",
            "Kham mat",
            totalAmount,
            1,
            Department.Medical);

        return invoice;
    }

    private CashierShift CreateOpenShift()
    {
        return CashierShift.Create(
            DefaultUserId,
            "Cashier 1",
            1_000_000m,
            null,
            new BranchId(DefaultBranchId));
    }

    private void SetupInvoiceAndShift(Invoice invoice, CashierShift shift)
    {
        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);
        _cashierShiftRepository.GetCurrentOpenAsync(
                Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns(shift);
    }

    // =========================================================================
    // RecordPayment Tests
    // =========================================================================

    [Fact]
    public async Task RecordPayment_CashPayment_CreatesConfirmedPaymentAndUpdatesTotals()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(500_000m);
        var shift = CreateOpenShift();
        SetupInvoiceAndShift(invoice, shift);

        var command = new RecordPaymentCommand(
            InvoiceId: invoice.Id,
            Method: (int)PaymentMethod.Cash,
            Amount: 500_000m,
            ReferenceNumber: null,
            CardLast4: null,
            CardType: null,
            Notes: "Cash payment",
            TreatmentPackageId: null,
            IsSplitPayment: false,
            SplitSequence: null);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Method.Should().Be((int)PaymentMethod.Cash);
        result.Value.Amount.Should().Be(500_000m);
        result.Value.Status.Should().Be((int)PaymentStatus.Confirmed);

        // Verify invoice.RecordPayment was called (PaidAmount updated)
        invoice.PaidAmount.Should().Be(500_000m);

        // Verify shift cash received updated
        shift.CashReceived.Should().Be(500_000m);
        shift.TotalRevenue.Should().BeGreaterThan(0);

        // Verify persistence
        _paymentRepository.Received(1).Add(Arg.Any<Payment>());
        _invoiceRepository.Received(1).Update(invoice);
        _cashierShiftRepository.Received(1).Update(shift);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordPayment_BankTransfer_RecordsReferenceNumber()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(300_000m);
        var shift = CreateOpenShift();
        SetupInvoiceAndShift(invoice, shift);

        var command = new RecordPaymentCommand(
            InvoiceId: invoice.Id,
            Method: (int)PaymentMethod.BankTransfer,
            Amount: 300_000m,
            ReferenceNumber: "VCB-2026-123456",
            CardLast4: null,
            CardType: null,
            Notes: null,
            TreatmentPackageId: null,
            IsSplitPayment: false,
            SplitSequence: null);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Method.Should().Be((int)PaymentMethod.BankTransfer);
        result.Value.ReferenceNumber.Should().Be("VCB-2026-123456");
        result.Value.Amount.Should().Be(300_000m);

        // Non-cash: shift NonCashRevenue updated, not CashReceived
        shift.CashReceived.Should().Be(0);
        shift.TotalRevenue.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RecordPayment_QrPayment_RecordsMethodAndReference()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(200_000m);
        var shift = CreateOpenShift();
        SetupInvoiceAndShift(invoice, shift);

        var command = new RecordPaymentCommand(
            InvoiceId: invoice.Id,
            Method: (int)PaymentMethod.QrMomo,
            Amount: 200_000m,
            ReferenceNumber: "MOMO-TXN-789",
            CardLast4: null,
            CardType: null,
            Notes: null,
            TreatmentPackageId: null,
            IsSplitPayment: false,
            SplitSequence: null);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Method.Should().Be((int)PaymentMethod.QrMomo);
        result.Value.ReferenceNumber.Should().Be("MOMO-TXN-789");

        // Non-cash: shift NonCashRevenue updated
        shift.CashReceived.Should().Be(0);
        shift.TotalRevenue.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RecordPayment_CardPayment_RecordsCardDetails()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(400_000m);
        var shift = CreateOpenShift();
        SetupInvoiceAndShift(invoice, shift);

        var command = new RecordPaymentCommand(
            InvoiceId: invoice.Id,
            Method: (int)PaymentMethod.CardVisa,
            Amount: 400_000m,
            ReferenceNumber: null,
            CardLast4: "4242",
            CardType: "Visa",
            Notes: null,
            TreatmentPackageId: null,
            IsSplitPayment: false,
            SplitSequence: null);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Method.Should().Be((int)PaymentMethod.CardVisa);
        result.Value.CardLast4.Should().Be("4242");
        result.Value.CardType.Should().Be("Visa");

        // Non-cash: shift NonCashRevenue updated
        shift.CashReceived.Should().Be(0);
        shift.TotalRevenue.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RecordPayment_VoidedInvoice_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(500_000m);
        invoice.Void();

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var command = new RecordPaymentCommand(
            InvoiceId: invoice.Id,
            Method: (int)PaymentMethod.Cash,
            Amount: 500_000m,
            ReferenceNumber: null,
            CardLast4: null,
            CardType: null,
            Notes: null,
            TreatmentPackageId: null,
            IsSplitPayment: false,
            SplitSequence: null);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("voided");
    }

    [Fact]
    public async Task RecordPayment_ExceedsBalance_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(500_000m);
        var shift = CreateOpenShift();
        SetupInvoiceAndShift(invoice, shift);

        var command = new RecordPaymentCommand(
            InvoiceId: invoice.Id,
            Method: (int)PaymentMethod.Cash,
            Amount: 600_000m, // Exceeds TotalAmount of 500,000
            ReferenceNumber: null,
            CardLast4: null,
            CardType: null,
            Notes: null,
            TreatmentPackageId: null,
            IsSplitPayment: false,
            SplitSequence: null);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Payment exceeds balance due");
    }

    [Fact]
    public async Task RecordPayment_TreatmentSplitPayment_MarksAsSplitWithSequence()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(1_000_000m);
        var shift = CreateOpenShift();
        SetupInvoiceAndShift(invoice, shift);
        var treatmentPackageId = Guid.NewGuid();

        var command = new RecordPaymentCommand(
            InvoiceId: invoice.Id,
            Method: (int)PaymentMethod.Cash,
            Amount: 500_000m, // 50% of total
            ReferenceNumber: null,
            CardLast4: null,
            CardType: null,
            Notes: "First payment for treatment package",
            TreatmentPackageId: treatmentPackageId,
            IsSplitPayment: true,
            SplitSequence: 1);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TreatmentPackageId.Should().Be(treatmentPackageId);
        result.Value.IsSplitPayment.Should().BeTrue();
        result.Value.SplitSequence.Should().Be(1);
        result.Value.Amount.Should().Be(500_000m);

        // Verify the payment was persisted
        _paymentRepository.Received(1).Add(Arg.Is<Payment>(p =>
            p.TreatmentPackageId == treatmentPackageId &&
            p.IsSplitPayment == true &&
            p.SplitSequence == 1));
    }

    [Fact]
    public async Task RecordPayment_InvoiceNotFound_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var invoiceId = Guid.NewGuid();
        _invoiceRepository.GetByIdAsync(invoiceId, Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        var command = new RecordPaymentCommand(
            InvoiceId: invoiceId,
            Method: (int)PaymentMethod.Cash,
            Amount: 100_000m,
            ReferenceNumber: null,
            CardLast4: null,
            CardType: null,
            Notes: null,
            TreatmentPackageId: null,
            IsSplitPayment: false,
            SplitSequence: null);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task RecordPayment_NoOpenShift_ReturnsError()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateDraftInvoice(500_000m);
        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);
        _cashierShiftRepository.GetCurrentOpenAsync(
                Arg.Any<BranchId>(), Arg.Any<CancellationToken>())
            .Returns((CashierShift?)null);

        var command = new RecordPaymentCommand(
            InvoiceId: invoice.Id,
            Method: (int)PaymentMethod.Cash,
            Amount: 500_000m,
            ReferenceNumber: null,
            CardLast4: null,
            CardType: null,
            Notes: null,
            TreatmentPackageId: null,
            IsSplitPayment: false,
            SplitSequence: null);

        // Act
        var result = await RecordPaymentHandler.Handle(
            command, _invoiceRepository, _paymentRepository,
            _cashierShiftRepository, _unitOfWork, _validator,
            _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("shift");
    }

    // =========================================================================
    // GetPaymentsByInvoice Tests
    // =========================================================================

    [Fact]
    public async Task GetPaymentsByInvoice_ValidInvoiceId_ReturnsPaymentList()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            Payment.Create(invoiceId, PaymentMethod.Cash, 200_000m, DefaultUserId),
            Payment.Create(invoiceId, PaymentMethod.BankTransfer, 300_000m, DefaultUserId,
                referenceNumber: "VCB-123")
        };
        _paymentRepository.GetByInvoiceIdAsync(invoiceId, Arg.Any<CancellationToken>())
            .Returns(payments);

        var query = new GetPaymentsByInvoiceQuery(invoiceId);

        // Act
        var result = await GetPaymentsByInvoiceHandler.Handle(
            query, _paymentRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].InvoiceId.Should().Be(invoiceId);
        result[0].Method.Should().Be((int)PaymentMethod.Cash);
        result[1].Method.Should().Be((int)PaymentMethod.BankTransfer);
        result[1].ReferenceNumber.Should().Be("VCB-123");
    }

    [Fact]
    public async Task GetPaymentsByInvoice_NoPayments_ReturnsEmptyList()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        _paymentRepository.GetByInvoiceIdAsync(invoiceId, Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        var query = new GetPaymentsByInvoiceQuery(invoiceId);

        // Act
        var result = await GetPaymentsByInvoiceHandler.Handle(
            query, _paymentRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
