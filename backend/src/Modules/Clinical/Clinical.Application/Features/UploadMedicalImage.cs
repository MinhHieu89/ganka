using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentValidation;
using Shared.Application;
using Shared.Application.Services;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Content type whitelist for medical image uploads.
/// Images: JPEG, PNG, TIFF, BMP, WebP. Videos: MP4, MOV, AVI.
/// </summary>
internal static class AllowedContentTypes
{
    public static readonly HashSet<string> Images = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/tiff", "image/bmp", "image/webp"
    };

    public static readonly HashSet<string> Videos = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/quicktime", "video/x-msvideo"
    };

    public static readonly HashSet<string> All = [.. Images, .. Videos];

    public const long MaxImageSize = 50L * 1024 * 1024;  // 50 MB
    public const long MaxVideoSize = 200L * 1024 * 1024;  // 200 MB
}

/// <summary>
/// Validator for <see cref="UploadMedicalImageCommand"/>.
/// Validates file size (images max 50MB, video max 200MB), content type whitelist,
/// and required VisitId.
/// </summary>
public class UploadMedicalImageCommandValidator : AbstractValidator<UploadMedicalImageCommand>
{
    public UploadMedicalImageCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.FileName).NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.All.Contains(ct))
            .WithMessage(x => $"File type '{x.ContentType}' is not allowed.");

        // Size validation depends on image type
        RuleFor(x => x.FileSize).Custom((fileSize, context) =>
        {
            var imageType = (ImageType)context.InstanceToValidate.ImageType;
            if (imageType == ImageType.Video)
            {
                if (fileSize > AllowedContentTypes.MaxVideoSize)
                    context.AddFailure("FileSize", "Video file size must not exceed 200 MB.");
            }
            else
            {
                if (fileSize > AllowedContentTypes.MaxImageSize)
                    context.AddFailure("FileSize", "Image file size must not exceed 50 MB.");
            }
        });
    }
}

/// <summary>
/// Wolverine handler for uploading a medical image to Azure Blob Storage.
/// Creates MedicalImage metadata in DB. Does NOT check EnsureEditable -- images are append-only.
/// </summary>
public static class UploadMedicalImageHandler
{
    public static async Task<Result<Guid>> Handle(
        UploadMedicalImageCommand command,
        IVisitRepository visitRepository,
        IMedicalImageRepository imageRepository,
        IAzureBlobService blobService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IValidator<UploadMedicalImageCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<Guid>(Error.ValidationWithDetails(errors));
        }

        // Verify visit exists (lightweight GetByIdAsync, not GetByIdWithDetailsAsync)
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure<Guid>(Error.NotFound("Visit", command.VisitId));

        // NOTE: No EnsureEditable check -- images are append-only even after sign-off

        // Generate blob name
        var sanitizedFileName = SanitizeFileName(command.FileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var blobName = $"clinical-images/{command.VisitId}/{timestamp}_{sanitizedFileName}";

        // Upload to Azure Blob Storage
        await blobService.UploadAsync("clinical-images", blobName, command.Stream, command.ContentType);

        // Create metadata entity
        var imageType = (ImageType)command.ImageType;
        var eyeTag = command.EyeTag.HasValue ? (EyeTag?)command.EyeTag.Value : null;

        var image = MedicalImage.Create(
            command.VisitId,
            currentUser.UserId,
            imageType,
            eyeTag,
            command.FileName,
            blobName,
            command.ContentType,
            command.FileSize);

        await imageRepository.AddAsync(image, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return image.Id;
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove path separators and dangerous characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "upload" : sanitized;
    }
}
