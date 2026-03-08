namespace Optical.Contracts.Dtos;

/// <summary>
/// Combo package DTO for API serialization.
/// Represents preset admin-created frame+lens combinations with discounted pricing.
/// Savings is computed: OriginalTotalPrice - ComboPrice (null when OriginalTotalPrice is null).
/// </summary>
public sealed record ComboPackageDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? FrameId,
    string? FrameName,
    Guid? LensCatalogItemId,
    string? LensName,
    decimal ComboPrice,
    decimal? OriginalTotalPrice,
    decimal? Savings,
    bool IsActive,
    DateTime CreatedAt);
