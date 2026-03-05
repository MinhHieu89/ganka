using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Shared.Application.Services;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for querying medical images by visit ID.
/// Returns DTOs with SAS URLs for secure client-side image access.
/// </summary>
public static class GetVisitImagesHandler
{
    private static readonly TimeSpan SasExpiry = TimeSpan.FromHours(1);

    public static async Task<List<MedicalImageDto>> Handle(
        GetVisitImagesQuery query,
        IMedicalImageRepository imageRepository,
        IAzureBlobService blobService,
        CancellationToken ct)
    {
        var images = await imageRepository.GetByVisitIdAsync(query.VisitId, ct);

        var dtos = new List<MedicalImageDto>(images.Count);
        foreach (var image in images)
        {
            var sasUrl = await blobService.GetSasUrlAsync("clinical-images", image.BlobName, SasExpiry);

            dtos.Add(new MedicalImageDto(
                image.Id,
                image.VisitId,
                (int)image.Type,
                image.EyeTag.HasValue ? (int?)image.EyeTag.Value : null,
                image.OriginalFileName,
                sasUrl,
                image.ContentType,
                image.FileSize,
                image.Description,
                image.CreatedAt));
        }

        return dtos;
    }
}
