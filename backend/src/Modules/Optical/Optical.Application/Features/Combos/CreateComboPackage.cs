using Shared.Domain;

namespace Optical.Application.Features.Combos;

/// <summary>
/// Command to create a preset combo package (admin).
/// Handler implementation provided in plan 08-19.
/// </summary>
public sealed record CreateComboPackageCommand(
    string Name,
    string? Description,
    Guid? FrameId,
    Guid? LensCatalogItemId,
    decimal ComboPrice,
    decimal? OriginalTotalPrice);
