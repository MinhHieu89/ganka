namespace Pharmacy.Contracts.Dtos;

/// <summary>
/// Cross-module DTO for a dispensing record linked to a clinical prescription.
/// </summary>
public sealed record DispensingRecordDto(
    Guid Id,
    Guid PrescriptionId,
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    DateTime DispensedAt,
    string? OverrideReason,
    List<DispensingLineDto> Lines);

/// <summary>
/// Cross-module DTO for a single dispensing line within a dispensing record.
/// Status is int-serialized Pharmacy.Domain.Enums.DispensingStatus.
/// </summary>
public sealed record DispensingLineDto(
    Guid Id,
    Guid DrugCatalogItemId,
    string DrugName,
    int Quantity,
    int Status,
    List<BatchDeductionDto> BatchDeductions);

/// <summary>
/// Cross-module DTO for a batch deduction record linking a dispensing line to a specific drug batch.
/// </summary>
public sealed record BatchDeductionDto(
    Guid Id,
    Guid DrugBatchId,
    string BatchNumber,
    int Quantity);

/// <summary>
/// Cross-module DTO for a walk-in OTC sale without prescription.
/// </summary>
public sealed record OtcSaleDto(
    Guid Id,
    Guid? PatientId,
    string? CustomerName,
    DateTime SoldAt,
    string? Notes,
    List<OtcSaleLineDto> Lines);

/// <summary>
/// Cross-module DTO for a single line in an OTC sale.
/// </summary>
public sealed record OtcSaleLineDto(
    Guid Id,
    Guid DrugCatalogItemId,
    string DrugName,
    int Quantity,
    decimal UnitPrice);

/// <summary>
/// Cross-module DTO for a prescription pending dispensing in the pharmacy queue.
/// IsExpired indicates the 7-day validity window has passed.
/// </summary>
public sealed record PendingPrescriptionDto(
    Guid PrescriptionId,
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    DateTime PrescribedAt,
    bool IsExpired,
    int DaysRemaining,
    List<PendingPrescriptionItemDto> Items);

/// <summary>
/// Cross-module DTO for a single drug item within a pending prescription.
/// IsOffCatalog indicates the drug was manually typed and has no catalog entry.
/// </summary>
public sealed record PendingPrescriptionItemDto(
    Guid PrescriptionItemId,
    Guid? DrugCatalogItemId,
    string DrugName,
    int Quantity,
    string Unit,
    string? Dosage,
    bool IsOffCatalog);
