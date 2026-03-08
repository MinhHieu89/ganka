using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Line item within a glasses order -- frame or lens selection.
/// </summary>
public sealed record GlassesOrderItemRequest(
    Guid? FrameId,
    Guid? LensCatalogItemId,
    string Description,
    decimal UnitPrice,
    int Quantity);

/// <summary>
/// Command to create a new glasses order linked to a patient visit and optical prescription.
/// Handler implementation provided in plan 08-18.
/// </summary>
public sealed record CreateGlassesOrderCommand(
    Guid PatientId,
    Guid VisitId,
    Guid OpticalPrescriptionId,
    int ProcessingType,
    DateTime? EstimatedDeliveryDate,
    Guid? ComboPackageId,
    string? Notes,
    List<GlassesOrderItemRequest> Items);
