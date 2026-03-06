namespace Pharmacy.Contracts.Dtos;

/// <summary>
/// Cross-module DTO for a consumable item in the warehouse.
/// TrackingMode is int-serialized Pharmacy.Domain.Enums.ConsumableTrackingMode:
///   0 = ExpiryTracked (batch model with FEFO)
///   1 = SimpleStock (quantity-only)
/// </summary>
public sealed record ConsumableItemDto(
    Guid Id,
    string Name,
    string NameVi,
    string Unit,
    int TrackingMode,
    int CurrentStock,
    int MinStockLevel,
    bool IsActive,
    bool IsLowStock);

/// <summary>
/// Cross-module DTO for a consumable batch (only for ExpiryTracked consumables).
/// IsExpired and IsNearExpiry are computed fields from the backend.
/// </summary>
public sealed record ConsumableBatchDto(
    Guid Id,
    Guid ConsumableItemId,
    string BatchNumber,
    DateOnly ExpiryDate,
    int InitialQuantity,
    int CurrentQuantity,
    bool IsExpired,
    bool IsNearExpiry);
