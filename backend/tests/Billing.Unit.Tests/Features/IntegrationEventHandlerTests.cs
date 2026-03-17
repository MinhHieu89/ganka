using Billing.Application.Features;
using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Clinical.Contracts.IntegrationEvents;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Optical.Contracts.IntegrationEvents;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Contracts.IntegrationEvents;
using Shared.Domain;
using Treatment.Contracts.IntegrationEvents;
using Wolverine;

namespace Billing.Unit.Tests.Features;

public class IntegrationEventHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IServiceCatalogRepository _serviceCatalogRepository = Substitute.For<IServiceCatalogRepository>();
    private readonly IBillingNotificationService _notificationService = Substitute.For<IBillingNotificationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger _visitCreatedLogger = Substitute.For<ILogger>();
    private readonly ILogger _treatmentLogger = Substitute.For<ILogger>();
    private readonly ILogger _cancelledLogger = Substitute.For<ILogger>();
    private readonly ILogger _drugLogger = Substitute.For<ILogger>();
    private readonly ILogger _otcLogger = Substitute.For<ILogger>();
    private readonly ILogger _glassesLogger = Substitute.For<ILogger>();
    private readonly ILogger _prescriptionLogger = Substitute.For<ILogger>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public IntegrationEventHandlerTests()
    {
        _invoiceRepository.GetNextInvoiceNumberAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns("HD-2026-00001");
    }

    private Invoice CreateTestInvoice(Guid? visitId = null, Guid? patientId = null)
    {
        return Invoice.Create(
            "HD-2026-00001",
            patientId ?? Guid.NewGuid(),
            "Test Patient",
            visitId,
            new BranchId(DefaultBranchId));
    }

    #region HandleVisitCreated Tests

    [Fact]
    public async Task HandleVisitCreated_WithConsultationService_CreatesInvoiceWithLineItem()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var @event = new VisitCreatedIntegrationEvent(visitId, patientId, "Nguyen Van A", DefaultBranchId);

        var consultationService = ServiceCatalogItem.Create(
            "CONSULTATION", "Consultation", "Kham benh", 150000m, new BranchId(DefaultBranchId));
        _serviceCatalogRepository.GetActiveByCodeAsync("CONSULTATION", Arg.Any<CancellationToken>())
            .Returns(consultationService);

        // Act
        await HandleVisitCreatedHandler.Handle(
            @event, _invoiceRepository, _serviceCatalogRepository, _notificationService, _unitOfWork,
            _visitCreatedLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.VisitId == visitId &&
            inv.PatientId == patientId &&
            inv.PatientName == "Nguyen Van A" &&
            inv.Status == InvoiceStatus.Draft &&
            inv.LineItems.Count == 1 &&
            inv.LineItems[0].Department == Department.Medical &&
            inv.LineItems[0].UnitPrice == 150000m &&
            inv.LineItems[0].Quantity == 1 &&
            inv.LineItems[0].SourceType == "Visit"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleVisitCreated_WithoutConsultationService_CreatesInvoiceWithoutLineItem()
    {
        // Arrange
        var @event = new VisitCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "Patient", DefaultBranchId);

        _serviceCatalogRepository.GetActiveByCodeAsync("CONSULTATION", Arg.Any<CancellationToken>())
            .Returns((ServiceCatalogItem?)null);

        // Act
        await HandleVisitCreatedHandler.Handle(
            @event, _invoiceRepository, _serviceCatalogRepository, _notificationService, _unitOfWork,
            _visitCreatedLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.LineItems.Count == 0));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleVisitCreated_UsesNextInvoiceNumber()
    {
        // Arrange
        var @event = new VisitCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "Patient", DefaultBranchId);
        _serviceCatalogRepository.GetActiveByCodeAsync("CONSULTATION", Arg.Any<CancellationToken>())
            .Returns((ServiceCatalogItem?)null);
        _invoiceRepository.GetNextInvoiceNumberAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns("HD-2026-00042");

        // Act
        await HandleVisitCreatedHandler.Handle(
            @event, _invoiceRepository, _serviceCatalogRepository, _notificationService, _unitOfWork,
            _visitCreatedLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.InvoiceNumber == "HD-2026-00042"));
    }

    #endregion

    #region HandleVisitCancelled Tests

    [Fact]
    public async Task HandleVisitCancelled_DraftInvoice_VoidsInvoice()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var @event = new VisitCancelledIntegrationEvent(visitId, DefaultBranchId);

        // Act
        await HandleVisitCancelledHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _cancelledLogger, CancellationToken.None);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Voided);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleVisitCancelled_NoInvoice_NoOp()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        var @event = new VisitCancelledIntegrationEvent(visitId, DefaultBranchId);

        // Act
        await HandleVisitCancelledHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _cancelledLogger, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleVisitCancelled_AlreadyVoided_NoOp()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        invoice.Void(); // already voided
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var @event = new VisitCancelledIntegrationEvent(visitId, DefaultBranchId);

        // Act
        await HandleVisitCancelledHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _cancelledLogger, CancellationToken.None);

        // Assert - SaveChanges should not be called since already voided
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleVisitCancelled_FinalizedInvoice_NoOp()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        // Add a line item, pay, confirm, then finalize to get Finalized status
        invoice.AddLineItem("Consultation", "Kham benh", 150000m, 1, Department.Medical, visitId, "Visit");
        var payment = Payment.Create(invoice.Id, PaymentMethod.Cash, 150000m, Guid.NewGuid());
        payment.Confirm();
        invoice.RecordPayment(payment);
        invoice.Finalize(Guid.NewGuid(), Guid.NewGuid());

        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var @event = new VisitCancelledIntegrationEvent(visitId, DefaultBranchId);

        // Act
        await HandleVisitCancelledHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _cancelledLogger, CancellationToken.None);

        // Assert - Finalized invoice should NOT be voided, no SaveChanges
        invoice.Status.Should().Be(InvoiceStatus.Finalized);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region HandleDrugDispensed Tests

    [Fact]
    public async Task HandleDrugDispensed_ExistingInvoice_AddsLineItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var items = new List<DrugDispensedIntegrationEvent.DrugLineDto>
        {
            new("Amoxicillin", "Amoxicillin VN", 2, 50000m),
            new("Ibuprofen", "Ibuprofen VN", 1, 30000m)
        };
        var @event = new DrugDispensedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleDrugDispensedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _drugLogger, CancellationToken.None);

        // Assert
        invoice.LineItems.Should().HaveCount(2);
        invoice.LineItems[0].Description.Should().Be("Amoxicillin");
        invoice.LineItems[0].DescriptionVi.Should().Be("Amoxicillin VN");
        invoice.LineItems[0].UnitPrice.Should().Be(50000m);
        invoice.LineItems[0].Quantity.Should().Be(2);
        invoice.LineItems[0].Department.Should().Be(Department.Pharmacy);
        invoice.LineItems[0].SourceType.Should().Be("Dispensing");
        invoice.LineItems[1].Description.Should().Be("Ibuprofen");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleDrugDispensed_NoExistingInvoice_CreatesInvoiceThenAdds()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        var items = new List<DrugDispensedIntegrationEvent.DrugLineDto>
        {
            new("Amoxicillin", "Amoxicillin VN", 1, 50000m)
        };
        var @event = new DrugDispensedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleDrugDispensedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _drugLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.VisitId == visitId &&
            inv.LineItems.Count == 1 &&
            inv.LineItems[0].Department == Department.Pharmacy));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region HandleOtcSaleCompleted Tests

    [Fact]
    public async Task HandleOtcSaleCompleted_CreatesStandaloneInvoice()
    {
        // Arrange
        var otcSaleId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var items = new List<OtcSaleCompletedIntegrationEvent.DrugLineDto>
        {
            new("Eye Drops", "Thuoc nho mat", 1, 120000m),
            new("Vitamins", "Vitamin", 2, 80000m)
        };
        var @event = new OtcSaleCompletedIntegrationEvent(otcSaleId, patientId, "Customer Name", items, DefaultBranchId);

        // Act
        await HandleOtcSaleCompletedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _otcLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.VisitId == null &&
            inv.PatientId == patientId &&
            inv.PatientName == "Customer Name" &&
            inv.LineItems.Count == 2 &&
            inv.LineItems[0].Department == Department.Pharmacy &&
            inv.LineItems[0].SourceType == "OtcSale" &&
            inv.LineItems[1].Description == "Vitamins"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleOtcSaleCompleted_AnonymousCustomer_UsesNullPatientId()
    {
        // Arrange
        var otcSaleId = Guid.NewGuid();
        var items = new List<OtcSaleCompletedIntegrationEvent.DrugLineDto>
        {
            new("Eye Drops", "Thuoc nho mat", 1, 120000m)
        };
        var @event = new OtcSaleCompletedIntegrationEvent(otcSaleId, null, null, items, DefaultBranchId);

        // Act
        await HandleOtcSaleCompletedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _otcLogger, CancellationToken.None);

        // Assert - PatientId should be null, NOT Guid.Empty
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.VisitId == null &&
            inv.PatientId == null &&
            inv.PatientName == "Anonymous" &&
            inv.LineItems.Count == 1));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleDrugDispensed_DuplicateEvent_DoesNotAddDuplicateLineItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        // Pre-add line items that simulate a previous delivery of the same event
        invoice.AddLineItem("Amoxicillin", "Amoxicillin VN", 50000m, 2, Department.Pharmacy, visitId, "Dispensing");
        invoice.AddLineItem("Ibuprofen", "Ibuprofen VN", 30000m, 1, Department.Pharmacy, visitId, "Dispensing");
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var items = new List<DrugDispensedIntegrationEvent.DrugLineDto>
        {
            new("Amoxicillin", "Amoxicillin VN", 2, 50000m),
            new("Ibuprofen", "Ibuprofen VN", 1, 30000m)
        };
        var @event = new DrugDispensedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleDrugDispensedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _drugLogger, CancellationToken.None);

        // Assert - should still have exactly 2 line items, not 4
        invoice.LineItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleDrugDispensed_ZeroPriceItem_SkipsLineItem()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var items = new List<DrugDispensedIntegrationEvent.DrugLineDto>
        {
            new("Free Sample", "Mau thu mien phi", 1, 0m),
            new("Amoxicillin", "Amoxicillin VN", 2, 50000m)
        };
        var @event = new DrugDispensedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleDrugDispensedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _drugLogger, CancellationToken.None);

        // Assert - only the non-zero price item should be added
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].Description.Should().Be("Amoxicillin");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region HandleGlassesOrderCreated Tests

    [Fact]
    public async Task HandleGlassesOrderCreated_ExistingInvoice_AddsLineItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var items = new List<GlassesOrderCreatedIntegrationEvent.OrderLineDto>
        {
            new("Frame - Rayban", "Gong kinh - Rayban", 500000m, 1),
            new("Lens - Progressive", "Trong kinh da tieu cu", 1200000m, 2)
        };
        var @event = new GlassesOrderCreatedIntegrationEvent(orderId, visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleGlassesOrderCreatedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _glassesLogger, CancellationToken.None);

        // Assert
        invoice.LineItems.Should().HaveCount(2);
        invoice.LineItems[0].Description.Should().Be("Frame - Rayban");
        invoice.LineItems[0].DescriptionVi.Should().Be("Gong kinh - Rayban");
        invoice.LineItems[0].UnitPrice.Should().Be(500000m);
        invoice.LineItems[0].Quantity.Should().Be(1);
        invoice.LineItems[0].Department.Should().Be(Department.Optical);
        invoice.LineItems[0].SourceType.Should().Be("GlassesOrder");
        invoice.LineItems[1].Description.Should().Be("Lens - Progressive");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleGlassesOrderCreated_NoExistingInvoice_CreatesInvoiceThenAdds()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        var items = new List<GlassesOrderCreatedIntegrationEvent.OrderLineDto>
        {
            new("Frame", "Gong kinh", 500000m, 1)
        };
        var @event = new GlassesOrderCreatedIntegrationEvent(orderId, visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleGlassesOrderCreatedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _glassesLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.VisitId == visitId &&
            inv.LineItems.Count == 1 &&
            inv.LineItems[0].Department == Department.Optical));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleGlassesOrderCreated_DuplicateEvent_DoesNotAddDuplicateLineItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        // Pre-add line items that simulate a previous delivery of the same event
        invoice.AddLineItem("Frame - Rayban", "Gong kinh - Rayban", 500000m, 1, Department.Optical, orderId, "GlassesOrder");
        invoice.AddLineItem("Lens - Progressive", "Trong kinh da tieu cu", 1200000m, 2, Department.Optical, orderId, "GlassesOrder");
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var items = new List<GlassesOrderCreatedIntegrationEvent.OrderLineDto>
        {
            new("Frame - Rayban", "Gong kinh - Rayban", 500000m, 1),
            new("Lens - Progressive", "Trong kinh da tieu cu", 1200000m, 2)
        };
        var @event = new GlassesOrderCreatedIntegrationEvent(orderId, visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleGlassesOrderCreatedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _glassesLogger, CancellationToken.None);

        // Assert - should still have exactly 2 line items, not 4
        invoice.LineItems.Should().HaveCount(2);
    }

    #endregion

    #region HandleTreatmentSessionCompleted Tests

    [Fact]
    public async Task HandleTreatmentSessionCompleted_WithVisitId_AddsSessionFeeLineItem()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var @event = new TreatmentSessionCompletedIntegrationEvent(
            Guid.NewGuid(), sessionId, Guid.NewGuid(), "Test Patient", (int)TreatmentType.IPL,
            new List<TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto>(),
            visitId, 200000m, DefaultBranchId);

        // Act
        await HandleTreatmentSessionCompletedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _treatmentLogger, CancellationToken.None);

        // Assert
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].UnitPrice.Should().Be(200000m);
        invoice.LineItems[0].Quantity.Should().Be(1);
        invoice.LineItems[0].Department.Should().Be(Department.Treatment);
        invoice.LineItems[0].SourceType.Should().Be("TreatmentSession");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleTreatmentSessionCompleted_NullVisitId_SkipsWithWarning()
    {
        // Arrange
        var @event = new TreatmentSessionCompletedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Patient", (int)TreatmentType.LLLT,
            new List<TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto>(),
            null, 150000m, DefaultBranchId);

        // Act
        await HandleTreatmentSessionCompletedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _treatmentLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.DidNotReceive().Add(Arg.Any<Invoice>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleTreatmentSessionCompleted_NoExistingInvoice_CreatesInvoiceThenAdds()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        var @event = new TreatmentSessionCompletedIntegrationEvent(
            Guid.NewGuid(), sessionId, patientId, "Test Patient", (int)TreatmentType.LidCare,
            new List<TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto>(),
            visitId, 100000m, DefaultBranchId);

        // Act
        await HandleTreatmentSessionCompletedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _treatmentLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.VisitId == visitId &&
            inv.PatientId == patientId &&
            inv.LineItems.Count == 1 &&
            inv.LineItems[0].Department == Department.Treatment &&
            inv.LineItems[0].UnitPrice == 100000m));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleTreatmentSessionCompleted_IPLType_CorrectDescription()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var @event = new TreatmentSessionCompletedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Patient", (int)TreatmentType.IPL,
            new List<TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto>(),
            visitId, 200000m, DefaultBranchId);

        // Act
        await HandleTreatmentSessionCompletedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _treatmentLogger, CancellationToken.None);

        // Assert
        invoice.LineItems[0].Description.Should().Contain("IPL");
    }

    #endregion

    #region HandleDrugPrescriptionAdded Tests

    [Fact]
    public async Task HandleDrugPrescriptionAdded_NoExistingInvoice_CreatesInvoiceWithLineItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        _messageBus.InvokeAsync<List<DrugCatalogPriceDto>>(
            Arg.Any<GetDrugCatalogPricesQuery>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan?>())
            .Returns([new DrugCatalogPriceDto(catalogItemId, 50000m, "Amoxicillin VN")]);

        var items = new List<DrugPrescriptionAddedIntegrationEvent.PrescribedDrugDto>
        {
            new("Amoxicillin", catalogItemId, 6)
        };
        var @event = new DrugPrescriptionAddedIntegrationEvent(visitId, patientId, "Patient", DefaultBranchId, items);

        // Act
        await HandleDrugPrescriptionAddedHandler.Handle(
            @event, _invoiceRepository, _messageBus, _notificationService, _unitOfWork,
            _prescriptionLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.VisitId == visitId &&
            inv.PatientId == patientId &&
            inv.LineItems.Count == 1 &&
            inv.LineItems[0].Department == Department.Pharmacy &&
            inv.LineItems[0].SourceType == "Prescription" &&
            inv.LineItems[0].UnitPrice == 50000m &&
            inv.LineItems[0].Quantity == 6));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleDrugPrescriptionAdded_ExistingInvoice_AddsLineItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        _messageBus.InvokeAsync<List<DrugCatalogPriceDto>>(
            Arg.Any<GetDrugCatalogPricesQuery>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan?>())
            .Returns([new DrugCatalogPriceDto(catalogItemId, 75000m, "Ibuprofen VN")]);

        var items = new List<DrugPrescriptionAddedIntegrationEvent.PrescribedDrugDto>
        {
            new("Ibuprofen", catalogItemId, 2)
        };
        var @event = new DrugPrescriptionAddedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", DefaultBranchId, items);

        // Act
        await HandleDrugPrescriptionAddedHandler.Handle(
            @event, _invoiceRepository, _messageBus, _notificationService, _unitOfWork,
            _prescriptionLogger, CancellationToken.None);

        // Assert
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].Description.Should().Be("Ibuprofen");
        invoice.LineItems[0].DescriptionVi.Should().Be("Ibuprofen VN");
        invoice.LineItems[0].UnitPrice.Should().Be(75000m);
        invoice.LineItems[0].Quantity.Should().Be(2);
        invoice.LineItems[0].Department.Should().Be(Department.Pharmacy);
        invoice.LineItems[0].SourceType.Should().Be("Prescription");
    }

    [Fact]
    public async Task HandleDrugPrescriptionAdded_DuplicateEvent_DoesNotAddDuplicateItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        invoice.AddLineItem("Amoxicillin", "Amoxicillin VN", 50000m, 6, Department.Pharmacy, visitId, "Prescription");
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        _messageBus.InvokeAsync<List<DrugCatalogPriceDto>>(
            Arg.Any<GetDrugCatalogPricesQuery>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan?>())
            .Returns([]);

        var items = new List<DrugPrescriptionAddedIntegrationEvent.PrescribedDrugDto>
        {
            new("Amoxicillin", Guid.NewGuid(), 6)
        };
        var @event = new DrugPrescriptionAddedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", DefaultBranchId, items);

        // Act
        await HandleDrugPrescriptionAddedHandler.Handle(
            @event, _invoiceRepository, _messageBus, _notificationService, _unitOfWork,
            _prescriptionLogger, CancellationToken.None);

        // Assert - should still have exactly 1 line item, not 2
        invoice.LineItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleDrugPrescriptionAdded_OffCatalogItem_UsesZeroPrice()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        _messageBus.InvokeAsync<List<DrugCatalogPriceDto>>(
            Arg.Any<GetDrugCatalogPricesQuery>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan?>())
            .Returns([]);

        var items = new List<DrugPrescriptionAddedIntegrationEvent.PrescribedDrugDto>
        {
            new("Custom Eye Drops", null, 1) // off-catalog, no DrugCatalogItemId
        };
        var @event = new DrugPrescriptionAddedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", DefaultBranchId, items);

        // Act
        await HandleDrugPrescriptionAddedHandler.Handle(
            @event, _invoiceRepository, _messageBus, _notificationService, _unitOfWork,
            _prescriptionLogger, CancellationToken.None);

        // Assert - off-catalog items get price 0 (cashier adjusts manually)
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].UnitPrice.Should().Be(0m);
        invoice.LineItems[0].Description.Should().Be("Custom Eye Drops");
    }

    [Fact]
    public async Task HandleDrugPrescriptionAdded_SendsSignalRNotification()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        _messageBus.InvokeAsync<List<DrugCatalogPriceDto>>(
            Arg.Any<GetDrugCatalogPricesQuery>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan?>())
            .Returns([new DrugCatalogPriceDto(catalogItemId, 50000m, null)]);

        var items = new List<DrugPrescriptionAddedIntegrationEvent.PrescribedDrugDto>
        {
            new("Amoxicillin", catalogItemId, 6)
        };
        var @event = new DrugPrescriptionAddedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", DefaultBranchId, items);

        // Act
        await HandleDrugPrescriptionAddedHandler.Handle(
            @event, _invoiceRepository, _messageBus, _notificationService, _unitOfWork,
            _prescriptionLogger, CancellationToken.None);

        // Assert
        await _notificationService.Received(1).NotifyLineItemAddedAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), "Amoxicillin", Arg.Any<decimal>(), "Pharmacy", Arg.Any<CancellationToken>());
    }

    #endregion

    #region HandleDrugPrescriptionRemoved Tests

    [Fact]
    public async Task HandleDrugPrescriptionRemoved_RemovesPrescriptionLineItems()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        invoice.AddLineItem("Amoxicillin", "Amoxicillin VN", 50000m, 6, Department.Pharmacy, visitId, "Prescription");
        invoice.AddLineItem("Ibuprofen", "Ibuprofen VN", 30000m, 2, Department.Pharmacy, visitId, "Prescription");
        invoice.AddLineItem("Consultation", "Kham benh", 150000m, 1, Department.Medical, visitId, "Visit");
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var @event = new DrugPrescriptionRemovedIntegrationEvent(visitId, DefaultBranchId, ["Amoxicillin", "Ibuprofen"]);

        // Act
        await HandleDrugPrescriptionRemovedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork,
            _prescriptionLogger, CancellationToken.None);

        // Assert
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].Description.Should().Be("Consultation");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleDrugPrescriptionRemoved_NoInvoice_DoesNothing()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        var @event = new DrugPrescriptionRemovedIntegrationEvent(visitId, DefaultBranchId, ["Amoxicillin"]);

        // Act
        await HandleDrugPrescriptionRemovedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork,
            _prescriptionLogger, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleDrugPrescriptionRemoved_NoMatchingItems_DoesNothing()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        invoice.AddLineItem("Consultation", "Kham benh", 150000m, 1, Department.Medical, visitId, "Visit");
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var @event = new DrugPrescriptionRemovedIntegrationEvent(visitId, DefaultBranchId, ["Amoxicillin"]);

        // Act
        await HandleDrugPrescriptionRemovedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork,
            _prescriptionLogger, CancellationToken.None);

        // Assert
        invoice.LineItems.Should().HaveCount(1);
        // SaveChanges may or may not be called depending on implementation, but no items removed
    }

    [Fact]
    public async Task RemoveInvoiceLineItem_PrescriptionSourced_ReturnsError()
    {
        // Arrange
        var invoice = CreateTestInvoice(Guid.NewGuid());
        invoice.AddLineItem("Amoxicillin", "Amoxicillin VN", 50000m, 6, Department.Pharmacy, Guid.NewGuid(), "Prescription");
        var lineItemId = invoice.LineItems[0].Id;

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var validator = Substitute.For<IValidator<RemoveInvoiceLineItemCommand>>();
        validator.ValidateAsync(Arg.Any<RemoveInvoiceLineItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());

        var command = new RemoveInvoiceLineItemCommand(invoice.Id, lineItemId);

        // Act
        var result = await RemoveInvoiceLineItemHandler.Handle(
            command, _invoiceRepository, _unitOfWork, validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("prescription");
    }

    #endregion

    #region HandleDrugDispensed Idempotency with Prescription Tests

    [Fact]
    public async Task HandleDrugDispensed_ItemAlreadyBilledFromPrescription_SkipsItem()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        // Pre-add line items from prescription
        invoice.AddLineItem("Amoxicillin", "Amoxicillin VN", 50000m, 6, Department.Pharmacy, visitId, "Prescription");
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var items = new List<DrugDispensedIntegrationEvent.DrugLineDto>
        {
            new("Amoxicillin", "Amoxicillin VN", 6, 50000m)
        };
        var @event = new DrugDispensedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleDrugDispensedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _drugLogger, CancellationToken.None);

        // Assert - should still have exactly 1 line item (from prescription), not 2
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].SourceType.Should().Be("Prescription");
    }

    [Fact]
    public async Task HandleDrugDispensed_PrescriptionItemWithZeroPrice_UpdatesPrice()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var invoice = CreateTestInvoice(visitId);
        // Pre-add line item from prescription with zero price (off-catalog or unknown price)
        invoice.AddLineItem("Custom Drug", null, 0m, 2, Department.Pharmacy, visitId, "Prescription");
        _invoiceRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var items = new List<DrugDispensedIntegrationEvent.DrugLineDto>
        {
            new("Custom Drug", "Thuoc tu che", 2, 35000m)
        };
        var @event = new DrugDispensedIntegrationEvent(visitId, Guid.NewGuid(), "Patient", items, DefaultBranchId);

        // Act
        await HandleDrugDispensedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _drugLogger, CancellationToken.None);

        // Assert - should still have 1 line item but with updated price and source type
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].UnitPrice.Should().Be(35000m);
        invoice.LineItems[0].SourceType.Should().Be("Dispensing");
    }

    #endregion
}

/// <summary>
/// Local enum mirror for test readability. Matches Treatment.Domain.Enums.TreatmentType values.
/// </summary>
file enum TreatmentType
{
    IPL = 0,
    LLLT = 1,
    LidCare = 2
}
