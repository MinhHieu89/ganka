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

namespace Billing.Unit.Tests.Features;

public class RefundHandlerTests
{
    private static readonly BranchId DefaultBranchId =
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<RequestRefundCommand> _validator = Substitute.For<IValidator<RequestRefundCommand>>();

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

    [Fact]
    public async Task RequestRefund_FinalizedInvoice_CreatesRequestedRefund()
    {
        // Arrange
        SetupValidValidator();
        var invoice = CreateFinalizedInvoice(500_000m, 1); // TotalAmount = 500,000
        var command = new RequestRefundCommand(
            invoice.Id, null, 100_000m, "Customer complaint", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RequestRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _validator, CancellationToken.None);

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
            invoice.Id, null, 100_000m, "Refund attempt on draft", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RequestRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _validator, CancellationToken.None);

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
            invoice.Id, null, 999_999m, "Excessive refund", Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await RequestRefundHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }
}
