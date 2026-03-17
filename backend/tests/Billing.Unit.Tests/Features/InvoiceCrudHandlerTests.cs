using Billing.Application.Features;
using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Billing.Contracts.Queries;
using Shared.Application;
using Shared.Domain;

namespace Billing.Unit.Tests.Features;

public class InvoiceCrudHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private readonly IValidator<CreateInvoiceCommand> _createValidator =
        Substitute.For<IValidator<CreateInvoiceCommand>>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public InvoiceCrudHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
        _currentUser.UserId.Returns(DefaultUserId);
    }

    private void SetupValidCreateValidator()
    {
        _createValidator
            .ValidateAsync(Arg.Any<CreateInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    #region CreateInvoice Tests

    [Fact]
    public async Task CreateInvoice_ValidInput_CreatesInvoiceWithGeneratedNumber()
    {
        // Arrange
        SetupValidCreateValidator();
        var patientId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var command = new CreateInvoiceCommand(patientId, "Nguyen Van A", visitId);

        _invoiceRepository.GetNextInvoiceNumberAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns("HD-2026-00001");

        // Act
        var result = await CreateInvoiceHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _createValidator, _currentUser,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.InvoiceNumber.Should().Be("HD-2026-00001");
        result.Value.PatientId.Should().Be(patientId);
        result.Value.PatientName.Should().Be("Nguyen Van A");
        result.Value.VisitId.Should().Be(visitId);
        result.Value.Status.Should().Be((int)InvoiceStatus.Draft);
        _invoiceRepository.Received(1).Add(Arg.Any<Invoice>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInvoice_MissingPatientId_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateInvoiceCommand(Guid.Empty, "Nguyen Van A", null);
        _createValidator
            .ValidateAsync(Arg.Any<CreateInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("PatientId", "Patient ID is required.")
            }));

        // Act
        var result = await CreateInvoiceHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _createValidator, _currentUser,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    #endregion

    #region AddInvoiceLineItem Tests

    [Fact]
    public async Task AddInvoiceLineItem_DraftInvoice_AddsItemAndRecalculatesTotals()
    {
        // Arrange
        var invoice = Invoice.Create(
            "HD-2026-00001", Guid.NewGuid(), "Nguyen Van A", Guid.NewGuid(),
            new BranchId(DefaultBranchId));

        var command = new AddInvoiceLineItemCommand(
            InvoiceId: invoice.Id,
            Description: "Eye Examination",
            DescriptionVi: "Kham mat",
            UnitPrice: 200000m,
            Quantity: 1,
            Department: (int)Department.Medical,
            SourceId: null,
            SourceType: null);

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);

        // Act
        var result = await AddInvoiceLineItemHandler.Handle(
            command, _invoiceRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.LineItems.Should().HaveCount(1);
        result.Value.LineItems[0].Description.Should().Be("Eye Examination");
        result.Value.LineItems[0].Department.Should().Be((int)Department.Medical);
        result.Value.SubTotal.Should().Be(200000m);
        result.Value.TotalAmount.Should().Be(200000m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddInvoiceLineItem_FinalizedInvoice_ReturnsError()
    {
        // Arrange
        var invoice = CreateFinalizedInvoice();

        var command = new AddInvoiceLineItemCommand(
            InvoiceId: invoice.Id,
            Description: "Eye Drops",
            DescriptionVi: "Thuoc nho mat",
            UnitPrice: 50000m,
            Quantity: 2,
            Department: (int)Department.Pharmacy,
            SourceId: null,
            SourceType: null);

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);

        // Act
        var result = await AddInvoiceLineItemHandler.Handle(
            command, _invoiceRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().ContainAny("not in draft status", "Draft");
    }

    #endregion

    #region FinalizeInvoice Tests

    [Fact]
    public async Task FinalizeInvoice_FullyPaid_SetsStatusAndRecordsFinalizer()
    {
        // Arrange
        var invoice = CreateFullyPaidInvoice();
        var cashierShiftId = Guid.NewGuid();

        var command = new FinalizeInvoiceCommand(invoice.Id, cashierShiftId);

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);

        // Act
        var result = await FinalizeInvoiceHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Finalized);
        invoice.CashierShiftId.Should().Be(cashierShiftId);
        invoice.FinalizedById.Should().Be(DefaultUserId);
        invoice.FinalizedAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizeInvoice_OutstandingBalance_ReturnsError()
    {
        // Arrange - invoice with line item but no payment (has outstanding balance)
        var invoice = Invoice.Create(
            "HD-2026-00003", Guid.NewGuid(), "Tran Van C", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Consultation", "Kham benh", 500000m, 1, Department.Medical);

        var command = new FinalizeInvoiceCommand(invoice.Id, Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);

        // Act
        var result = await FinalizeInvoiceHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("outstanding balance");
    }

    [Fact]
    public async Task FinalizeInvoice_EmptyInvoice_ReturnsError()
    {
        // Arrange -- invoice with no line items
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        var command = new FinalizeInvoiceCommand(invoice.Id, Guid.NewGuid());

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await FinalizeInvoiceHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("no line items");
    }

    [Fact]
    public async Task FinalizeInvoice_ZeroTotal_ReturnsError()
    {
        // Arrange -- invoice with no line items (zero total)
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        // Add and remove a line item to get zero total with tested path
        invoice.AddLineItem("Temp", "Temp", 100m, 1, Department.Medical);
        invoice.RemoveLineItem(invoice.LineItems[0].Id);

        var command = new FinalizeInvoiceCommand(invoice.Id, Guid.NewGuid());
        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await FinalizeInvoiceHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().ContainAny("no line items", "zero or negative total");
    }

    [Fact]
    public async Task FinalizeInvoice_WithLineItemsAndFullPayment_Succeeds()
    {
        // Arrange
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Eye Exam", "Kham mat", 500_000m, 1, Department.Medical);
        var payment = Payment.Create(invoice.Id, PaymentMethod.Cash, 500_000m, DefaultUserId);
        payment.Confirm();
        invoice.RecordPayment(payment);

        var command = new FinalizeInvoiceCommand(invoice.Id, Guid.NewGuid());
        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        // Act
        var result = await FinalizeInvoiceHandler.Handle(
            command, _invoiceRepository, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region GetPendingInvoices Tests

    [Fact]
    public async Task GetPendingInvoices_ReturnsDraftInvoices()
    {
        // Arrange
        var invoice1 = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Patient A", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        var invoice2 = Invoice.Create("HD-2026-00002", Guid.NewGuid(), "Patient B", null,
            new BranchId(DefaultBranchId));

        _invoiceRepository.GetPendingAsync(null, Arg.Any<CancellationToken>())
            .Returns(new List<Invoice> { invoice1, invoice2 });

        var query = new GetPendingInvoicesQuery();

        // Act
        var result = await GetPendingInvoicesHandler.Handle(
            query, _invoiceRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].InvoiceNumber.Should().Be("HD-2026-00001");
        result.Value[1].InvoiceNumber.Should().Be("HD-2026-00002");
    }

    [Fact]
    public async Task GetPendingInvoices_NoDraftInvoices_ReturnsEmptyList()
    {
        // Arrange
        _invoiceRepository.GetPendingAsync(null, Arg.Any<CancellationToken>())
            .Returns(new List<Invoice>());

        var query = new GetPendingInvoicesQuery();

        // Act
        var result = await GetPendingInvoicesHandler.Handle(
            query, _invoiceRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region GetAllInvoices Tests

    [Fact]
    public async Task GetAllInvoices_NoFilter_ReturnsAllInvoices()
    {
        // Arrange
        var invoice1 = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Patient A", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        var invoice2 = CreateFinalizedInvoice();

        _invoiceRepository.GetAllAsync(null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Invoice> { invoice1, invoice2 }, 2));

        var query = new GetAllInvoicesQuery(null, null, 1, 20);

        // Act
        var result = await GetAllInvoicesHandler.Handle(query, _invoiceRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetAllInvoices_FilterByStatus_ReturnsFilteredInvoices()
    {
        // Arrange - filter for Draft (0) only
        var draftInvoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Patient A", Guid.NewGuid(),
            new BranchId(DefaultBranchId));

        _invoiceRepository.GetAllAsync(0, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Invoice> { draftInvoice }, 1));

        var query = new GetAllInvoicesQuery(0, null, 1, 20);

        // Act
        var result = await GetAllInvoicesHandler.Handle(query, _invoiceRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Status.Should().Be((int)InvoiceStatus.Draft);
    }

    [Fact]
    public async Task GetAllInvoices_SearchByPatientName_ReturnsMatchingInvoices()
    {
        // Arrange
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Nguyen Van A", Guid.NewGuid(),
            new BranchId(DefaultBranchId));

        _invoiceRepository.GetAllAsync(null, "Nguyen", 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Invoice> { invoice }, 1));

        var query = new GetAllInvoicesQuery(null, "Nguyen", 1, 20);

        // Act
        var result = await GetAllInvoicesHandler.Handle(query, _invoiceRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].PatientName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public async Task GetAllInvoices_SearchByInvoiceNumber_ReturnsMatchingInvoices()
    {
        // Arrange
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Patient A", Guid.NewGuid(),
            new BranchId(DefaultBranchId));

        _invoiceRepository.GetAllAsync(null, "HD-2026-00001", 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Invoice> { invoice }, 1));

        var query = new GetAllInvoicesQuery(null, "HD-2026-00001", 1, 20);

        // Act
        var result = await GetAllInvoicesHandler.Handle(query, _invoiceRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].InvoiceNumber.Should().Be("HD-2026-00001");
    }

    [Fact]
    public async Task GetAllInvoices_Pagination_ReturnsCorrectPageInfo()
    {
        // Arrange - 25 total items, requesting page 2 with page size 10
        var invoices = Enumerable.Range(1, 10)
            .Select(i => Invoice.Create($"HD-2026-{i:D5}", Guid.NewGuid(), $"Patient {i}", Guid.NewGuid(),
                new BranchId(DefaultBranchId)))
            .ToList();

        _invoiceRepository.GetAllAsync(null, null, 2, 10, Arg.Any<CancellationToken>())
            .Returns((invoices, 25));

        var query = new GetAllInvoicesQuery(null, null, 2, 10);

        // Act
        var result = await GetAllInvoicesHandler.Handle(query, _invoiceRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(25);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(10);
    }

    #endregion

    #region RemoveLineItem Guard Tests

    [Fact]
    public void RemoveLineItem_WithPrescriptionSource_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Amoxicillin", "Amoxicillin VN", 50000m, 6, Department.Pharmacy, Guid.NewGuid(), "Prescription");
        var lineItemId = invoice.LineItems[0].Id;

        // Act & Assert
        var act = () => invoice.RemoveLineItem(lineItemId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*prescription*");
    }

    [Fact]
    public void RemoveLineItem_WithNullSource_Succeeds()
    {
        // Arrange
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Eye Exam", "Kham mat", 200000m, 1, Department.Medical);
        var lineItemId = invoice.LineItems[0].Id;

        // Act
        invoice.RemoveLineItem(lineItemId);

        // Assert
        invoice.LineItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveLineItem_WithDispensingSource_Succeeds()
    {
        // Arrange
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Amoxicillin", "Amoxicillin VN", 50000m, 2, Department.Pharmacy, Guid.NewGuid(), "Dispensing");
        var lineItemId = invoice.LineItems[0].Id;

        // Act
        invoice.RemoveLineItem(lineItemId);

        // Assert
        invoice.LineItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveLineItemsBySource_RemovesMatchingItems_And_RecalculatesTotals()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Amoxicillin", null, 50000m, 6, Department.Pharmacy, visitId, "Prescription");
        invoice.AddLineItem("Ibuprofen", null, 30000m, 2, Department.Pharmacy, visitId, "Prescription");
        invoice.AddLineItem("Consultation", null, 150000m, 1, Department.Medical, visitId, "Visit");

        // Act
        invoice.RemoveLineItemsBySource(visitId, "Prescription");

        // Assert
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].Description.Should().Be("Consultation");
        invoice.SubTotal.Should().Be(150000m);
    }

    [Fact]
    public void RemoveLineItemsBySource_NoMatching_NoChanges()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = Invoice.Create("HD-2026-00001", Guid.NewGuid(), "Test", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Consultation", null, 150000m, 1, Department.Medical, visitId, "Visit");

        // Act
        invoice.RemoveLineItemsBySource(Guid.NewGuid(), "Prescription");

        // Assert
        invoice.LineItems.Should().HaveCount(1);
        invoice.SubTotal.Should().Be(150000m);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a finalized invoice for testing operations that should be rejected on non-draft invoices.
    /// </summary>
    private Invoice CreateFinalizedInvoice()
    {
        var invoice = Invoice.Create(
            "HD-2026-00002", Guid.NewGuid(), "Le Thi B", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Eye Exam", "Kham mat", 200000m, 1, Department.Medical);

        // Record confirmed payment to make fully paid
        var payment = Payment.Create(
            invoice.Id, PaymentMethod.Cash, 200000m, DefaultUserId, Guid.NewGuid());
        payment.Confirm();
        invoice.RecordPayment(payment);

        // Finalize
        invoice.Finalize(Guid.NewGuid(), DefaultUserId);

        return invoice;
    }

    /// <summary>
    /// Creates a fully paid draft invoice ready for finalization.
    /// </summary>
    private Invoice CreateFullyPaidInvoice()
    {
        var invoice = Invoice.Create(
            "HD-2026-00004", Guid.NewGuid(), "Pham Van D", Guid.NewGuid(),
            new BranchId(DefaultBranchId));
        invoice.AddLineItem("Laser Treatment", "Dieu tri laser", 1000000m, 1, Department.Treatment);

        // Record confirmed payment to cover full amount
        var payment = Payment.Create(
            invoice.Id, PaymentMethod.Cash, 1000000m, DefaultUserId, Guid.NewGuid());
        payment.Confirm();
        invoice.RecordPayment(payment);

        return invoice;
    }

    #endregion
}
