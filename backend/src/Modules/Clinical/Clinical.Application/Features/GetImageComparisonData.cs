using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Shared.Application.Services;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for cross-visit image comparison.
/// Returns images of the same type from two visits for side-by-side comparison.
/// Verifies both visits belong to the specified patient (security check).
/// </summary>
public static class GetImageComparisonDataHandler
{
    private static readonly TimeSpan SasExpiry = TimeSpan.FromHours(1);

    public static async Task<Result<ImageComparisonResponse>> Handle(
        GetImageComparisonQuery query,
        IVisitRepository visitRepository,
        IMedicalImageRepository imageRepository,
        IAzureBlobService blobService,
        CancellationToken ct)
    {
        // Verify both visits exist
        var visit1 = await visitRepository.GetByIdAsync(query.VisitId1, ct);
        if (visit1 is null)
            return Result.Failure<ImageComparisonResponse>(Error.NotFound("Visit", query.VisitId1));

        var visit2 = await visitRepository.GetByIdAsync(query.VisitId2, ct);
        if (visit2 is null)
            return Result.Failure<ImageComparisonResponse>(Error.NotFound("Visit", query.VisitId2));

        // Security check: both visits must belong to the same patient
        if (visit1.PatientId != query.PatientId || visit2.PatientId != query.PatientId)
            return Result.Failure<ImageComparisonResponse>(
                Error.Validation("Both visits must belong to the specified patient."));

        var imageType = (ImageType)query.ImageType;

        // Query images of the specified type for each visit
        var images1 = await imageRepository.GetByVisitIdAndTypeAsync(query.VisitId1, imageType, ct);
        var images2 = await imageRepository.GetByVisitIdAndTypeAsync(query.VisitId2, imageType, ct);

        // Map to DTOs with SAS URLs
        var visit1Dtos = await MapToImageDtos(images1, blobService);
        var visit2Dtos = await MapToImageDtos(images2, blobService);

        return new ImageComparisonResponse(visit1Dtos, visit2Dtos);
    }

    private static async Task<MedicalImageDto[]> MapToImageDtos(
        List<MedicalImage> images, IAzureBlobService blobService)
    {
        var dtos = new MedicalImageDto[images.Count];
        for (int i = 0; i < images.Count; i++)
        {
            var image = images[i];
            var sasUrl = await blobService.GetSasUrlAsync("clinical-images", image.BlobName, SasExpiry);

            dtos[i] = new MedicalImageDto(
                image.Id,
                image.VisitId,
                (int)image.Type,
                image.EyeTag.HasValue ? (int?)image.EyeTag.Value : null,
                image.OriginalFileName,
                sasUrl,
                image.ContentType,
                image.FileSize,
                image.Description,
                image.CreatedAt);
        }
        return dtos;
    }
}
