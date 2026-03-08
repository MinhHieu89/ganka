namespace Treatment.Contracts.Dtos;

/// <summary>
/// DTO representing a treatment package assigned to a patient.
/// Includes denormalized protocol and patient names, computed session counts,
/// scheduling info, and nested session/cancellation data.
/// Status, TreatmentType, and PricingMode are string representations of their respective enums.
/// </summary>
public sealed record TreatmentPackageDto(
    Guid Id,
    Guid ProtocolTemplateId,
    string ProtocolTemplateName,
    Guid PatientId,
    string PatientName,
    string TreatmentType,
    string Status,
    int TotalSessions,
    int SessionsCompleted,
    int SessionsRemaining,
    string PricingMode,
    decimal PackagePrice,
    decimal SessionPrice,
    int MinIntervalDays,
    string ParametersJson,
    Guid? VisitId,
    DateTime CreatedAt,
    DateTime? LastSessionDate,
    DateTime? NextDueDate,
    List<TreatmentSessionDto> Sessions,
    CancellationRequestDto? CancellationRequest);
