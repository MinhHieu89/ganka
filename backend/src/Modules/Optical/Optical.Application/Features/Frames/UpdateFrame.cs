using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Command to update an existing frame's details.
/// Handler implementation provided in plan 08-16.
/// </summary>
public sealed record UpdateFrameCommand(
    Guid Id,
    string Brand,
    string Model,
    string Color,
    int LensWidth,
    int BridgeWidth,
    int TempleLength,
    int Material,
    int FrameType,
    int Gender,
    decimal SellingPrice,
    decimal CostPrice,
    string? Barcode,
    int StockQuantity,
    int MinStockLevel,
    bool IsActive);
