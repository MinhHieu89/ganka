using Billing.Application.Features;
using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Domain;
using Wolverine;

namespace Billing.Unit.Tests.Features;

public class DiscountHandlerTests
{
    private static readonly BranchId DefaultBranchId =
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<ApplyDiscountCommand> _applyValidator = Substitute.For<IValidator<ApplyDiscountCommand>>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();

    private void SetupValidApplyValidator()
    {
        _applyValidator.ValidateAsync(Arg.Any<ApplyDiscountCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private Invoice CreateDraftInvoice(decimal lineItemUnitPrice = 500_000m, int quantity = 2)
    {
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test Patient", Guid.NewGuid(), DefaultBranchId);
        invoice.AddLineItem("Eye Exam", "Kham mat", lineItemUnitPrice, quantity, Department.Medical);
        return invoice;
    }

    private Invoice CreateFinalizedInvoice()
    {
        var invoice = CreateDraftInvoice(500_000m, 1);
        // Record confirmed payment to cover full amount so we can finalize
        var payment = Payment.Create(invoice.Id, PaymentMethod.Cash, invoice.TotalAmount, Guid.NewGuid());
        payment.Confirm();
        invoice.RecordPayment(payment);
        invoice.Finalize(Guid.NewGuid(), Guid.NewGuid());
        return invoice;
    }

    // ===== ApplyDiscount Tests =====

    [Fact]
    public async Task ApplyDiscount_PercentageDiscount_CalculatesCorrectAmount()
    {
        // Arrange
        SetupValidApplyValidator();
        var invoice = CreateDraftInvoice(500_000m, 2); // SubTotal = 1,000,000
        var command = new ApplyDiscountCommand(
            invoice.Id, null, (int)DiscountType.Percentage, 10m, "Loyalty discount", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApplyDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _applyValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CalculatedAmount.Should().Be(100_000m); // 10% of 1,000,000
        result.Value.Type.Should().Be((int)DiscountType.Percentage);
        result.Value.ApprovalStatus.Should().Be((int)ApprovalStatus.Pending);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyDiscount_FixedAmountDiscount_SetsCalculatedAmount()
    {
        // Arrange
        SetupValidApplyValidator();
        var invoice = CreateDraftInvoice(500_000m, 2); // SubTotal = 1,000,000
        var command = new ApplyDiscountCommand(
            invoice.Id, null, (int)DiscountType.FixedAmount, 50_000m, "Special offer", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApplyDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _applyValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CalculatedAmount.Should().Be(50_000m);
        result.Value.Type.Should().Be((int)DiscountType.FixedAmount);
    }

    [Fact]
    public async Task ApplyDiscount_LineItemDiscount_AppliesToSpecificItem()
    {
        // Arrange
        SetupValidApplyValidator();
        var invoice = CreateDraftInvoice(500_000m, 2);
        var lineItemId = invoice.LineItems[0].Id;
        var command = new ApplyDiscountCommand(
            invoice.Id, lineItemId, (int)DiscountType.Percentage, 20m, "Line item discount", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApplyDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _applyValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.InvoiceLineItemId.Should().Be(lineItemId);
        // 20% of line item total (500,000 * 2 = 1,000,000) for the line item
        result.Value.CalculatedAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ApplyDiscount_FinalizedInvoice_ReturnsError()
    {
        // Arrange
        SetupValidApplyValidator();
        var invoice = CreateFinalizedInvoice();
        var command = new ApplyDiscountCommand(
            invoice.Id, null, (int)DiscountType.Percentage, 10m, "Too late", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApplyDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _applyValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // ===== ApproveDiscount Tests =====

    [Fact]
    public async Task ApproveDiscount_ValidPin_ApprovesAndRecalculatesInvoice()
    {
        // Arrange
        var invoice = CreateDraftInvoice(500_000m, 2); // SubTotal = 1,000,000
        var discount = Discount.Create(
            invoice.Id, DiscountType.Percentage, 10m, "Test discount", Guid.NewGuid());
        discount.CalculateAmount(invoice.SubTotal);
        invoice.ApplyDiscount(discount);

        var managerId = Guid.NewGuid();
        var command = new ApproveDiscountCommand(invoice.Id, discount.Id, managerId, "1234");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        _messageBus.InvokeAsync<VerifyManagerPinResponse>(
            Arg.Any<VerifyManagerPinQuery>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyManagerPinResponse(true));

        // Act
        var result = await ApproveDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        discount.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
        discount.ApprovedById.Should().Be(managerId);
        // After approval, invoice DiscountTotal should reflect the approved discount
        invoice.DiscountTotal.Should().Be(100_000m);
        invoice.TotalAmount.Should().Be(900_000m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveDiscount_InvalidPin_ReturnsError()
    {
        // Arrange
        var invoice = CreateDraftInvoice(500_000m, 2);
        var discount = Discount.Create(
            invoice.Id, DiscountType.FixedAmount, 50_000m, "Test", Guid.NewGuid());
        discount.CalculateAmount(invoice.SubTotal);
        invoice.ApplyDiscount(discount);

        var command = new ApproveDiscountCommand(invoice.Id, discount.Id, Guid.NewGuid(), "wrong-pin");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        _messageBus.InvokeAsync<VerifyManagerPinResponse>(
            Arg.Any<VerifyManagerPinQuery>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyManagerPinResponse(false));

        // Act
        var result = await ApproveDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Invalid manager PIN");
    }
}
