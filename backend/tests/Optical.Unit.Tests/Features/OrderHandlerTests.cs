using Billing.Contracts.Dtos;
using Billing.Contracts.Queries;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Optical.Application.Features.Orders;
using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;
using Wolverine;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// Unit tests for GlassesOrder command and query handlers.
/// Covers: CreateGlassesOrder, UpdateOrderStatus (with payment gate), GetGlassesOrders,
/// GetGlassesOrderById, and GetOverdueOrders.
/// </summary>
public class OrderHandlerTests
{
    private readonly IGlassesOrderRepository _orderRepository = Substitute.For<IGlassesOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IValidator<CreateGlassesOrderCommand> _createOrderValidator = Substitute.For<IValidator<CreateGlassesOrderCommand>>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultPatientId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid DefaultVisitId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    private static readonly Guid DefaultPrescriptionId = Guid.Parse("00000000-0000-0000-0000-000000000030");

    public OrderHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
        _createOrderValidator.ValidateAsync(Arg.Any<CreateGlassesOrderCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    // -------------------------------------------------------------------------
    // Helper factory methods
    // -------------------------------------------------------------------------

    private static GlassesOrder CreateTestOrder(
        Guid? visitId = null,
        GlassesOrderStatus initialStatus = GlassesOrderStatus.Ordered,
        DateTime? estimatedDeliveryDate = null)
    {
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var order = GlassesOrder.Create(
            patientId: DefaultPatientId,
            patientName: "Nguyen Van A",
            visitId: visitId ?? DefaultVisitId,
            opticalPrescriptionId: DefaultPrescriptionId,
            processingType: ProcessingType.InHouse,
            estimatedDeliveryDate: estimatedDeliveryDate,
            totalPrice: 3_000_000m,
            comboPackageId: null,
            notes: null,
            branchId: branchId);

        // Advance to the requested initial status
        if (initialStatus >= GlassesOrderStatus.Processing)
        {
            order.ConfirmPayment();
            order.TransitionTo(GlassesOrderStatus.Processing);
        }
        if (initialStatus >= GlassesOrderStatus.Received)
            order.TransitionTo(GlassesOrderStatus.Received);
        if (initialStatus >= GlassesOrderStatus.Ready)
            order.TransitionTo(GlassesOrderStatus.Ready);
        if (initialStatus == GlassesOrderStatus.Delivered)
            order.TransitionTo(GlassesOrderStatus.Delivered);

        return order;
    }

    private static InvoiceDto CreatePaidInvoiceDto(decimal balanceDue = 0m) =>
        new(
            Id: Guid.NewGuid(),
            InvoiceNumber: "HD-2026-00001",
            VisitId: DefaultVisitId,
            PatientId: DefaultPatientId,
            PatientName: "Nguyen Van A",
            Status: 2, // Finalized
            SubTotal: 3_000_000m,
            DiscountTotal: 0m,
            TotalAmount: 3_000_000m,
            PaidAmount: 3_000_000m - balanceDue,
            BalanceDue: balanceDue,
            CashierShiftId: null,
            FinalizedById: null,
            FinalizedAt: null,
            CreatedAt: DateTime.UtcNow.AddHours(-1),
            LineItems: [],
            Payments: [],
            Discounts: [],
            Refunds: []);

    // =========================================================================
    // CreateGlassesOrder Tests
    // =========================================================================

    #region CreateGlassesOrder Tests

    [Fact]
    public async Task CreateGlassesOrder_ValidInput_CreatesOrderWithOrderedStatus()
    {
        // Arrange
        var command = new CreateGlassesOrderCommand(
            PatientId: DefaultPatientId,
            PatientName: "Nguyen Van A",
            VisitId: DefaultVisitId,
            OpticalPrescriptionId: DefaultPrescriptionId,
            ProcessingType: (int)ProcessingType.InHouse,
            EstimatedDeliveryDate: DateTime.UtcNow.AddDays(3),
            TotalPrice: 3_000_000m,
            ComboPackageId: null,
            Notes: "Test order",
            Items: [new GlassesOrderItemRequest(null, null, "Custom Lens", 1_500_000m, 2)]);

        // Act
        var result = await CreateGlassesOrderHandler.Handle(
            command, _orderRepository, _unitOfWork, _currentUser, _createOrderValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        _orderRepository.Received(1).Add(Arg.Any<GlassesOrder>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateGlassesOrder_MissingPatientId_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateGlassesOrderCommand(
            PatientId: Guid.Empty,
            PatientName: "Nguyen Van A",
            VisitId: DefaultVisitId,
            OpticalPrescriptionId: DefaultPrescriptionId,
            ProcessingType: (int)ProcessingType.InHouse,
            EstimatedDeliveryDate: null,
            TotalPrice: 3_000_000m,
            ComboPackageId: null,
            Notes: null,
            Items: []);

        _createOrderValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("PatientId", "Patient ID is required.") }));

        // Act
        var result = await CreateGlassesOrderHandler.Handle(
            command, _orderRepository, _unitOfWork, _currentUser, _createOrderValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _orderRepository.DidNotReceive().Add(Arg.Any<GlassesOrder>());
    }

    [Fact]
    public async Task CreateGlassesOrder_ZeroTotalPrice_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateGlassesOrderCommand(
            PatientId: DefaultPatientId,
            PatientName: "Nguyen Van A",
            VisitId: DefaultVisitId,
            OpticalPrescriptionId: DefaultPrescriptionId,
            ProcessingType: (int)ProcessingType.InHouse,
            EstimatedDeliveryDate: null,
            TotalPrice: 0m,
            ComboPackageId: null,
            Notes: null,
            Items: []);

        _createOrderValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("TotalPrice", "Total price must be greater than zero.") }));

        // Act
        var result = await CreateGlassesOrderHandler.Handle(
            command, _orderRepository, _unitOfWork, _currentUser, _createOrderValidator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _orderRepository.DidNotReceive().Add(Arg.Any<GlassesOrder>());
    }

    [Fact]
    public async Task CreateGlassesOrder_WithItems_AddsItemsToOrder()
    {
        // Arrange
        var frameId = Guid.NewGuid();
        var lensId = Guid.NewGuid();
        var command = new CreateGlassesOrderCommand(
            PatientId: DefaultPatientId,
            PatientName: "Nguyen Van A",
            VisitId: DefaultVisitId,
            OpticalPrescriptionId: DefaultPrescriptionId,
            ProcessingType: (int)ProcessingType.Outsourced,
            EstimatedDeliveryDate: DateTime.UtcNow.AddDays(5),
            TotalPrice: 5_000_000m,
            ComboPackageId: null,
            Notes: null,
            Items:
            [
                new GlassesOrderItemRequest(frameId, null, "Ray-Ban RB3025 Black", 2_000_000m, 1),
                new GlassesOrderItemRequest(null, lensId, "Essilor Crizal SV", 1_500_000m, 2)
            ]);

        GlassesOrder? capturedOrder = null;
        _orderRepository.When(r => r.Add(Arg.Any<GlassesOrder>()))
            .Do(call => capturedOrder = call.Arg<GlassesOrder>());

        // Act
        var result = await CreateGlassesOrderHandler.Handle(
            command, _orderRepository, _unitOfWork, _currentUser, _createOrderValidator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Items.Should().HaveCount(2);
        capturedOrder.Status.Should().Be(GlassesOrderStatus.Ordered);
        capturedOrder.ProcessingType.Should().Be(ProcessingType.Outsourced);
    }

    #endregion

    // =========================================================================
    // UpdateOrderStatus Tests
    // =========================================================================

    #region UpdateOrderStatus Tests

    [Fact]
    public async Task UpdateOrderStatus_ToProcessing_WithFullPayment_Succeeds()
    {
        // Arrange
        var order = CreateTestOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        _messageBus.InvokeAsync<InvoiceDto?>(
                Arg.Any<GetVisitInvoiceQuery>(), Arg.Any<CancellationToken>())
            .Returns(CreatePaidInvoiceDto(balanceDue: 0m));

        var command = new UpdateOrderStatusCommand(order.Id, (int)GlassesOrderStatus.Processing, null);

        // Act
        var result = await UpdateOrderStatusHandler.Handle(
            command, _orderRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(GlassesOrderStatus.Processing);
        order.IsPaymentConfirmed.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateOrderStatus_ToProcessing_WithOutstandingBalance_ReturnsFailure()
    {
        // Arrange
        var order = CreateTestOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        _messageBus.InvokeAsync<InvoiceDto?>(
                Arg.Any<GetVisitInvoiceQuery>(), Arg.Any<CancellationToken>())
            .Returns(CreatePaidInvoiceDto(balanceDue: 500_000m)); // Outstanding balance

        var command = new UpdateOrderStatusCommand(order.Id, (int)GlassesOrderStatus.Processing, null);

        // Act
        var result = await UpdateOrderStatusHandler.Handle(
            command, _orderRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Payment must be completed before processing");
        order.Status.Should().Be(GlassesOrderStatus.Ordered); // Status unchanged
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateOrderStatus_ToProcessing_WithNoInvoice_ReturnsFailure()
    {
        // Arrange
        var order = CreateTestOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        _messageBus.InvokeAsync<InvoiceDto?>(
                Arg.Any<GetVisitInvoiceQuery>(), Arg.Any<CancellationToken>())
            .Returns((InvoiceDto?)null); // No invoice found

        var command = new UpdateOrderStatusCommand(order.Id, (int)GlassesOrderStatus.Processing, null);

        // Act
        var result = await UpdateOrderStatusHandler.Handle(
            command, _orderRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Payment must be completed before processing");
        order.Status.Should().Be(GlassesOrderStatus.Ordered);
    }

    [Fact]
    public async Task UpdateOrderStatus_ProcessingToReceived_NoPaymentCheckRequired()
    {
        // Arrange
        var order = CreateTestOrder(initialStatus: GlassesOrderStatus.Processing);
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var command = new UpdateOrderStatusCommand(order.Id, (int)GlassesOrderStatus.Received, null);

        // Act
        var result = await UpdateOrderStatusHandler.Handle(
            command, _orderRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(GlassesOrderStatus.Received);
        await _messageBus.DidNotReceive().InvokeAsync<InvoiceDto?>(
            Arg.Any<GetVisitInvoiceQuery>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateOrderStatus_ReceivedToReady_NoPaymentCheckRequired()
    {
        // Arrange
        var order = CreateTestOrder(initialStatus: GlassesOrderStatus.Received);
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var command = new UpdateOrderStatusCommand(order.Id, (int)GlassesOrderStatus.Ready, null);

        // Act
        var result = await UpdateOrderStatusHandler.Handle(
            command, _orderRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(GlassesOrderStatus.Ready);
        await _messageBus.DidNotReceive().InvokeAsync<InvoiceDto?>(
            Arg.Any<GetVisitInvoiceQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateOrderStatus_OrderNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((GlassesOrder?)null);

        var command = new UpdateOrderStatusCommand(orderId, (int)GlassesOrderStatus.Processing, null);

        // Act
        var result = await UpdateOrderStatusHandler.Handle(
            command, _orderRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task UpdateOrderStatus_ReadyToDelivered_NoPaymentCheckRequired()
    {
        // Arrange
        var order = CreateTestOrder(initialStatus: GlassesOrderStatus.Ready);
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var command = new UpdateOrderStatusCommand(order.Id, (int)GlassesOrderStatus.Delivered, null);

        // Act
        var result = await UpdateOrderStatusHandler.Handle(
            command, _orderRepository, _unitOfWork, _messageBus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(GlassesOrderStatus.Delivered);
        await _messageBus.DidNotReceive().InvokeAsync<InvoiceDto?>(
            Arg.Any<GetVisitInvoiceQuery>(), Arg.Any<CancellationToken>());
    }

    #endregion

    // =========================================================================
    // GetGlassesOrders Tests
    // =========================================================================

    #region GetGlassesOrders Tests

    [Fact]
    public async Task GetGlassesOrders_NoFilter_ReturnsPaginatedList()
    {
        // Arrange
        var orders = new List<GlassesOrder>
        {
            CreateTestOrder(),
            CreateTestOrder()
        };
        _orderRepository.GetAllAsync(null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(orders);
        _orderRepository.GetTotalCountAsync(null, Arg.Any<CancellationToken>())
            .Returns(2);

        var query = new GetGlassesOrdersQuery(null, 1, 20);

        // Act
        var result = await GetGlassesOrdersHandler.Handle(
            query, _orderRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetGlassesOrders_WithStatusFilter_FiltersCorrectly()
    {
        // Arrange
        var orders = new List<GlassesOrder> { CreateTestOrder() };
        var statusFilter = (int)GlassesOrderStatus.Ordered;
        _orderRepository.GetAllAsync(statusFilter, 1, 10, Arg.Any<CancellationToken>())
            .Returns(orders);
        _orderRepository.GetTotalCountAsync(statusFilter, Arg.Any<CancellationToken>())
            .Returns(1);

        var query = new GetGlassesOrdersQuery(statusFilter, 1, 10);

        // Act
        var result = await GetGlassesOrdersHandler.Handle(
            query, _orderRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetGlassesOrders_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _orderRepository.GetAllAsync(null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new List<GlassesOrder>());
        _orderRepository.GetTotalCountAsync(null, Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new GetGlassesOrdersQuery(null, 1, 20);

        // Act
        var result = await GetGlassesOrdersHandler.Handle(
            query, _orderRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    #endregion

    // =========================================================================
    // GetGlassesOrderById Tests
    // =========================================================================

    #region GetGlassesOrderById Tests

    [Fact]
    public async Task GetGlassesOrderById_ExistingOrder_ReturnsOrderDto()
    {
        // Arrange
        var order = CreateTestOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var query = new GetGlassesOrderByIdQuery(order.Id);

        // Act
        var result = await GetGlassesOrderByIdHandler.Handle(
            query, _orderRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(order.Id);
        result.Value.PatientId.Should().Be(DefaultPatientId);
        result.Value.PatientName.Should().Be("Nguyen Van A");
        result.Value.Status.Should().Be((int)GlassesOrderStatus.Ordered);
    }

    [Fact]
    public async Task GetGlassesOrderById_NonExistentOrder_ReturnsNotFoundError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((GlassesOrder?)null);

        var query = new GetGlassesOrderByIdQuery(orderId);

        // Act
        var result = await GetGlassesOrderByIdHandler.Handle(
            query, _orderRepository, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GetGlassesOrderById_OrderWithItems_ReturnsItemsInDto()
    {
        // Arrange
        var branchId = new BranchId(DefaultBranchId);
        var order = GlassesOrder.Create(
            DefaultPatientId, "Nguyen Van A", DefaultVisitId, DefaultPrescriptionId,
            ProcessingType.InHouse, DateTime.UtcNow.AddDays(2),
            5_000_000m, null, null, branchId);

        var item = GlassesOrderItem.Create(
            order.Id, Guid.NewGuid(), null, "Test Frame", "Gong kinh test", 2_000_000m, 1);
        order.AddItem(item);

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var query = new GetGlassesOrderByIdQuery(order.Id);

        // Act
        var result = await GetGlassesOrderByIdHandler.Handle(
            query, _orderRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Description.Should().Be("Test Frame");
        result.Value.Items[0].UnitPrice.Should().Be(2_000_000m);
        result.Value.Items[0].Quantity.Should().Be(1);
        result.Value.Items[0].LineTotal.Should().Be(2_000_000m);
    }

    #endregion

    // =========================================================================
    // GetOverdueOrders Tests
    // =========================================================================

    #region GetOverdueOrders Tests

    [Fact]
    public async Task GetOverdueOrders_HasOverdueOrders_ReturnsList()
    {
        // Arrange
        var overdueOrder = CreateTestOrder(estimatedDeliveryDate: DateTime.UtcNow.AddDays(-2));
        _orderRepository.GetOverdueOrdersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<GlassesOrder> { overdueOrder });

        var query = new GetOverdueOrdersQuery();

        // Act
        var result = await GetOverdueOrdersHandler.Handle(
            query, _orderRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].IsOverdue.Should().BeTrue();
    }

    [Fact]
    public async Task GetOverdueOrders_NoOverdueOrders_ReturnsEmptyList()
    {
        // Arrange
        _orderRepository.GetOverdueOrdersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<GlassesOrder>());

        var query = new GetOverdueOrdersQuery();

        // Act
        var result = await GetOverdueOrdersHandler.Handle(
            query, _orderRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion
}
