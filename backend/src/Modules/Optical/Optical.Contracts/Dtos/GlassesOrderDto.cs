namespace Optical.Contracts.Dtos;

/// <summary>
/// Full glasses order DTO for API serialization.
/// Enum fields are int-serialized per established Billing pattern:
///   Status → Optical.Domain.Enums.GlassesOrderStatus (0=Ordered, 1=Processing, 2=Received, 3=Ready, 4=Delivered)
///   ProcessingType → Optical.Domain.Enums.ProcessingType (0=InHouse, 1=Outsourced)
/// IsOverdue is computed: EstimatedDeliveryDate &lt; today and Status != Delivered.
/// IsUnderWarranty is computed: DeliveredAt + 12 months > today and Status == Delivered.
/// </summary>
public sealed record GlassesOrderDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid VisitId,
    Guid OpticalPrescriptionId,
    int Status,
    int ProcessingType,
    bool IsPaymentConfirmed,
    DateTime? EstimatedDeliveryDate,
    DateTime? DeliveredAt,
    decimal TotalPrice,
    Guid? ComboPackageId,
    string? ComboPackageName,
    string? Notes,
    bool IsOverdue,
    bool IsUnderWarranty,
    List<GlassesOrderItemDto> Items,
    DateTime CreatedAt);

/// <summary>
/// Individual item within a glasses order (frame or lens selection).
/// FrameId/FrameName populated when item is a frame; LensCatalogItemId/LensName for lenses.
/// </summary>
public sealed record GlassesOrderItemDto(
    Guid Id,
    Guid? FrameId,
    string? FrameName,
    Guid? LensCatalogItemId,
    string? LensName,
    string Description,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

/// <summary>
/// Lightweight glasses order summary for list views and search results.
/// Omits detailed item lists for performance on large datasets.
/// </summary>
public sealed record GlassesOrderSummaryDto(
    Guid Id,
    string PatientName,
    int Status,
    int ProcessingType,
    decimal TotalPrice,
    bool IsPaymentConfirmed,
    DateTime? EstimatedDeliveryDate,
    bool IsOverdue,
    DateTime CreatedAt);
