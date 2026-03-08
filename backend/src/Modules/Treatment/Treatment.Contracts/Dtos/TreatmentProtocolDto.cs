namespace Treatment.Contracts.Dtos;

/// <summary>
/// DTO representing a treatment protocol template.
/// TreatmentType and PricingMode are string representations of their respective enums.
/// </summary>
public sealed record TreatmentProtocolDto(
    Guid Id,
    string Name,
    string TreatmentType,
    int DefaultSessionCount,
    string PricingMode,
    decimal DefaultPackagePrice,
    decimal DefaultSessionPrice,
    int MinIntervalDays,
    int MaxIntervalDays,
    string DefaultParametersJson,
    decimal CancellationDeductionPercent,
    bool IsActive,
    string? Description,
    DateTime CreatedAt);
