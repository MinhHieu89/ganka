using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Command to generate and assign an EAN-13 barcode to a frame.
/// </summary>
public sealed record GenerateBarcodeCommand(Guid FrameId);

/// <summary>
/// Wolverine static handler for auto-generating an EAN-13 barcode for a frame.
/// Uses Ean13Generator with clinic prefix and sequence number, validates uniqueness, persists.
/// </summary>
public static class GenerateBarcodeHandler
{
    public static async Task<Result<string>> Handle(
        GenerateBarcodeCommand command,
        IFrameRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var frame = await repository.GetByIdAsync(command.FrameId, ct);
        if (frame is null)
            return Result.Failure<string>(Error.NotFound("Frame", command.FrameId));

        var sequenceNumber = await repository.GetNextSequenceNumberAsync(ct);
        var barcode = Ean13Generator.Generate(sequenceNumber);

        var isUnique = await repository.IsBarcodeUniqueAsync(barcode, frame.Id, ct);
        if (!isUnique)
        {
            // Rare collision: try with a random offset
            barcode = Ean13Generator.Generate(sequenceNumber + new Random().Next(1, 1000));
            isUnique = await repository.IsBarcodeUniqueAsync(barcode, frame.Id, ct);
            if (!isUnique)
                return Result.Failure<string>(Error.Conflict("Could not generate a unique barcode. Please try again."));
        }

        frame.Update(
            frame.Brand,
            frame.Model,
            frame.Color,
            frame.LensWidth,
            frame.BridgeWidth,
            frame.TempleLength,
            frame.Material,
            frame.Type,
            frame.Gender,
            frame.SellingPrice,
            frame.CostPrice,
            barcode);

        await unitOfWork.SaveChangesAsync(ct);
        return barcode;
    }
}
