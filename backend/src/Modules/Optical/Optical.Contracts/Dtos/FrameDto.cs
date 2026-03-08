namespace Optical.Contracts.Dtos;

/// <summary>
/// Full frame DTO for API serialization.
/// Enum fields are int-serialized per established Billing pattern:
///   Material → Optical.Domain.Enums.FrameMaterial (0=Metal, 1=Plastic, 2=Titanium)
///   FrameType → Optical.Domain.Enums.FrameType (0=FullRim, 1=SemiRimless, 2=Rimless)
///   Gender → Optical.Domain.Enums.FrameGender (0=Male, 1=Female, 2=Unisex)
/// </summary>
public sealed record FrameDto(
    Guid Id,
    string Brand,
    string Model,
    string Color,
    int LensWidth,
    int BridgeWidth,
    int TempleLength,
    string SizeDisplay,
    int Material,
    int FrameType,
    int Gender,
    decimal SellingPrice,
    decimal CostPrice,
    string? Barcode,
    int StockQuantity,
    int MinStockLevel,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>
/// Lightweight frame summary for list views and search results.
/// </summary>
public sealed record FrameSummaryDto(
    Guid Id,
    string Brand,
    string Model,
    string Color,
    string SizeDisplay,
    int Material,
    int FrameType,
    int Gender,
    decimal SellingPrice,
    string? Barcode,
    int StockQuantity,
    bool IsActive);
