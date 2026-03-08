using Shared.Domain;

namespace Optical.Application.Features.Combos;

/// <summary>
/// Command to update an existing combo package.
/// Handler implementation provided in plan 08-19.
/// </summary>
public sealed record UpdateComboPackageCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? FrameId,
    Guid? LensCatalogItemId,
    decimal ComboPrice,
    decimal? OriginalTotalPrice,
    bool IsActive);
