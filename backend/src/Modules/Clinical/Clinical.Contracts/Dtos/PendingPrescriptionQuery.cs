namespace Clinical.Contracts.Dtos;

/// <summary>
/// Cross-module query to retrieve pending (not yet dispensed) drug prescriptions from the Clinical module.
/// Issued by Pharmacy.Infrastructure via IMessageBus to get the dispensing queue.
/// The handler lives in Clinical.Application and has access to DrugPrescription data.
/// </summary>
/// <param name="PatientId">Optional patient filter. When null, returns all pending prescriptions for the branch.</param>
public sealed record GetPendingPrescriptionsQuery(Guid? PatientId = null);

/// <summary>
/// Clinical-side pending prescription DTO for cross-module IMessageBus response.
/// Pharmacy.Infrastructure maps this to Pharmacy.Contracts.PendingPrescriptionDto.
/// </summary>
public sealed record ClinicalPendingPrescriptionDto(
    Guid PrescriptionId,
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    string? PrescriptionCode,
    DateTime PrescribedAt,
    bool IsExpired,
    int DaysRemaining,
    List<ClinicalPendingPrescriptionItemDto> Items);

/// <summary>
/// Individual item in a pending prescription (clinical-side cross-module DTO).
/// </summary>
public sealed record ClinicalPendingPrescriptionItemDto(
    Guid PrescriptionItemId,
    Guid? DrugCatalogItemId,
    string DrugName,
    int Quantity,
    string Unit,
    string? Dosage,
    bool IsOffCatalog);
