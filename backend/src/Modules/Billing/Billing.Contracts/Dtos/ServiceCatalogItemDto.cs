namespace Billing.Contracts.Dtos;

/// <summary>
/// DTO for service catalog item data transfer.
/// Used for API responses and cross-module communication.
/// </summary>
public sealed record ServiceCatalogItemDto(
    Guid Id,
    string Code,
    string Name,
    string NameVi,
    decimal Price,
    bool IsActive,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
