using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Application;
using Shared.Application.Services;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for deleting a medical image.
/// Removes both the blob from Azure Blob Storage and the metadata record from DB.
/// </summary>
public static class DeleteMedicalImageHandler
{
    public static async Task<Result> Handle(
        DeleteMedicalImageCommand command,
        IMedicalImageRepository imageRepository,
        IAzureBlobService blobService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var image = await imageRepository.GetByIdAsync(command.ImageId, ct);
        if (image is null)
            return Result.Failure(Error.NotFound("MedicalImage", command.ImageId));

        // Delete blob from Azure Blob Storage
        await blobService.DeleteAsync("clinical-images", image.BlobName);

        // Delete metadata from DB
        imageRepository.Delete(image);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
