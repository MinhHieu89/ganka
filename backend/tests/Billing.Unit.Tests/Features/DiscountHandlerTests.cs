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

public class DiscountHandlerTests
{
    private static readonly BranchId DefaultBranchId =
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<ApplyDiscountCommand> _applyValidator = Substitute.For<IValidator<ApplyDiscountCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public DiscountHandlerTests()
    {
        _currentUser.UserId.Returns(Guid.Parse("00000000-0000-0000-0000-000000000002"));
        _currentUser.BranchId.Returns(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    }

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
            invoice.Id, null, (int)DiscountType.Percentage, 10m, "Loyalty discount");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApplyDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _applyValidator, _currentUser, CancellationToken.None);

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
            invoice.Id, null, (int)DiscountType.FixedAmount, 50_000m, "Special offer");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApplyDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _applyValidator, _currentUser, CancellationToken.None);

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
            invoice.Id, lineItemId, (int)DiscountType.Percentage, 20m, "Line item discount");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApplyDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _applyValidator, _currentUser, CancellationToken.None);

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
            invoice.Id, null, (int)DiscountType.Percentage, 10m, "Too late");

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApplyDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _applyValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    // ===== ApproveDiscount Tests =====

    [Fact]
    public async Task ApproveDiscount_ApprovesAndRecalculatesInvoice()
    {
        // Arrange
        var invoice = CreateDraftInvoice(500_000m, 2); // SubTotal = 1,000,000
        var discount = Discount.Create(
            invoice.Id, DiscountType.Percentage, 10m, "Test discount", Guid.NewGuid());
        discount.CalculateAmount(invoice.SubTotal);
        invoice.ApplyDiscount(discount);

        var managerId = Guid.NewGuid();
        var command = new ApproveDiscountCommand(invoice.Id, discount.Id, managerId);

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await ApproveDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        discount.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
        discount.ApprovedById.Should().Be(managerId);
        // After approval, invoice DiscountTotal should reflect the approved discount
        invoice.DiscountTotal.Should().Be(100_000m);
        invoice.TotalAmount.Should().Be(900_000m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ===== RejectDiscount Tests =====

    [Fact]
    public async Task RejectDiscount_RejectsDiscountWithReason()
    {
        // Arrange
        var invoice = CreateDraftInvoice(500_000m, 2);
        var discount = Discount.Create(
            invoice.Id, DiscountType.Percentage, 10m, "Test discount", Guid.NewGuid());
        discount.CalculateAmount(invoice.SubTotal);
        invoice.ApplyDiscount(discount);

        var managerId = Guid.NewGuid();
        var command = new RejectDiscountCommand(
            invoice.Id, discount.Id, "Not eligible for this discount", managerId);

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RejectDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        discount.ApprovalStatus.Should().Be(ApprovalStatus.Rejected);
        discount.RejectionReason.Should().Be("Not eligible for this discount");
        // After rejection, invoice totals should be recalculated (rejected discount excluded)
        invoice.DiscountTotal.Should().Be(0m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectDiscount_AlreadyProcessed_ReturnsError()
    {
        // Arrange
        var invoice = CreateDraftInvoice(500_000m, 2);
        var discount = Discount.Create(
            invoice.Id, DiscountType.Percentage, 10m, "Test", Guid.NewGuid());
        discount.CalculateAmount(invoice.SubTotal);
        invoice.ApplyDiscount(discount);
        // Approve the discount first so it's no longer Pending
        discount.Approve(Guid.NewGuid());

        var command = new RejectDiscountCommand(
            invoice.Id, discount.Id, "Too late", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RejectDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already been processed");
    }

    [Fact]
    public async Task RejectDiscount_DiscountNotFound_ReturnsError()
    {
        // Arrange
        var invoice = CreateDraftInvoice(500_000m, 2);

        var command = new RejectDiscountCommand(
            invoice.Id, Guid.NewGuid(), "Not found", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RejectDiscountHandler.Handle(
            command, _invoiceRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
