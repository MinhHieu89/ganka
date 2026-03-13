using Billing.Application.Features;
using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Clinical.Contracts.IntegrationEvents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Optical.Contracts.IntegrationEvents;
using Pharmacy.Contracts.IntegrationEvents;
using Shared.Domain;
using Treatment.Contracts.IntegrationEvents;

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
    public async Task HandleGlassesOrderCreated_NullVisitId_CreatesStandaloneInvoice()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var items = new List<GlassesOrderCreatedIntegrationEvent.OrderLineDto>
        {
            new("Frame", "Gong kinh", 500000m, 1)
        };
        var @event = new GlassesOrderCreatedIntegrationEvent(orderId, null, patientId, "Patient", items, DefaultBranchId);

        // Act
        await HandleGlassesOrderCreatedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _glassesLogger, CancellationToken.None);

        // Assert
        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(inv =>
            inv.VisitId == null &&
            inv.LineItems.Count == 1));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
            Guid.NewGuid(), sessionId, Guid.NewGuid(), (int)TreatmentType.IPL,
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
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), (int)TreatmentType.LLLT,
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
            Guid.NewGuid(), sessionId, patientId, (int)TreatmentType.LidCare,
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
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), (int)TreatmentType.IPL,
            new List<TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto>(),
            visitId, 200000m, DefaultBranchId);

        // Act
        await HandleTreatmentSessionCompletedHandler.Handle(
            @event, _invoiceRepository, _notificationService, _unitOfWork, _treatmentLogger, CancellationToken.None);

        // Assert
        invoice.LineItems[0].Description.Should().Contain("IPL");
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
