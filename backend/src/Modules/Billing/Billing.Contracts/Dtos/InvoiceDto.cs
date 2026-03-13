namespace Billing.Contracts.Dtos;

/// <summary>
/// Full invoice DTO with line items, payments, discounts, and refunds.
/// Status is int-serialized Billing.Domain.Enums.InvoiceStatus.
/// </summary>
public sealed record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid? VisitId,
    Guid? PatientId,
    string PatientName,
    int Status,
    decimal SubTotal,
    decimal DiscountTotal,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    Guid? CashierShiftId,
    Guid? FinalizedById,
    DateTime? FinalizedAt,
    DateTime CreatedAt,
    List<InvoiceLineItemDto> LineItems,
    List<PaymentDto> Payments,
    List<DiscountDto> Discounts,
    List<RefundDto> Refunds);

/// <summary>
/// Individual line item within an invoice.
/// Department is int-serialized Billing.Domain.Enums.Department.
/// </summary>
public sealed record InvoiceLineItemDto(
    Guid Id,
    string Description,
    string? DescriptionVi,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    int Department,
    Guid? SourceId,
    string? SourceType);

/// <summary>
/// Discount applied to an invoice or specific line item.
/// Type is int-serialized Billing.Domain.Enums.DiscountType.
/// ApprovalStatus is int-serialized Billing.Domain.Enums.ApprovalStatus.
/// </summary>
public sealed record DiscountDto(
    Guid Id,
    Guid? InvoiceLineItemId,
    int Type,
    decimal Value,
    decimal CalculatedAmount,
    string Reason,
    int ApprovalStatus,
    Guid RequestedById,
    DateTime RequestedAt,
    Guid? ApprovedById,
    DateTime? ApprovedAt);

/// <summary>
/// Refund record for an invoice or specific line item.
/// Status is int-serialized Billing.Domain.Enums.RefundStatus.
/// </summary>
public sealed record RefundDto(
    Guid Id,
    Guid? InvoiceLineItemId,
    decimal Amount,
    string Reason,
    int Status,
    Guid RequestedById,
    DateTime RequestedAt,
    Guid? ApprovedById,
    DateTime? ApprovedAt,
    Guid? ProcessedById,
    DateTime? ProcessedAt);

/// <summary>
/// Lightweight invoice summary for list views and search results.
/// </summary>
public sealed record InvoiceSummaryDto(
    Guid Id,
    string InvoiceNumber,
    string PatientName,
    int Status,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    DateTime CreatedAt);
