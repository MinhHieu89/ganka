using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Command to record a barcode scan during a stocktaking session.
/// Upserts if the barcode was already scanned in this session.
/// Handler implementation provided in plan 08-20.
/// </summary>
public sealed record RecordStocktakingItemCommand(Guid SessionId, string Barcode, int PhysicalCount);
