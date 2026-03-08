using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Command to record a barcode scan during a stocktaking session.
/// Upserts if the barcode was already scanned in this session.
/// </summary>
public sealed record RecordStocktakingItemCommand(Guid SessionId, string Barcode, int PhysicalCount);

/// <summary>
/// Wolverine static handler for recording a barcode scan during a stocktaking session.
/// Resolves frame details from the barcode (if known) and upserts the item entry.
/// Unknown barcodes are recorded with null frame details and system count of 0.
/// </summary>
public static class RecordStocktakingItemHandler
{
    public static async Task<Result<StocktakingItemDto>> Handle(
        RecordStocktakingItemCommand command,
        IStocktakingRepository repository,
        IFrameRepository frameRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var session = await repository.GetByIdAsync(command.SessionId, ct);
        if (session is null)
            return Result.Failure<StocktakingItemDto>(
                Error.NotFound("StocktakingSession", command.SessionId));

        // Resolve frame by barcode (null if unknown barcode)
        var frame = await frameRepository.GetByBarcodeAsync(command.Barcode, ct);
        int systemCount = frame?.StockQuantity ?? 0;

        var item = session.RecordItem(
            barcode: command.Barcode,
            physicalCount: command.PhysicalCount,
            systemCount: systemCount,
            frameId: frame?.Id,
            frameName: frame is not null ? $"{frame.Brand} {frame.Model} {frame.Color}" : null);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new StocktakingItemDto(
            Id: item.Id,
            StocktakingSessionId: item.StocktakingSessionId,
            Barcode: item.Barcode,
            FrameId: item.FrameId,
            FrameName: item.FrameName,
            PhysicalCount: item.PhysicalCount,
            SystemCount: item.SystemCount,
            Discrepancy: item.Discrepancy));
    }
}
