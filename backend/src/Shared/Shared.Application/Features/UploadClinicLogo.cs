using Shared.Application.Interfaces;
using Shared.Application.Services;
using Shared.Domain;

namespace Shared.Application.Features;

/// <summary>
/// Command to upload a clinic logo image to Azure Blob Storage.
/// </summary>
public sealed record UploadClinicLogoCommand(
    Stream FileStream,
    string ContentType,
    string FileName);

/// <summary>
/// Wolverine handler for uploading a clinic logo.
/// Validates file size (max 5MB) and content type (image/jpeg, image/png, image/webp),
/// uploads to Azure Blob Storage, and updates ClinicSettings.LogoBlobUrl.
/// </summary>
public static class UploadClinicLogoHandler
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public static async Task<Result<string>> Handle(
        UploadClinicLogoCommand command,
        IAzureBlobService blobService,
        IClinicSettingsService settingsService,
        IBranchContext branchContext,
        CancellationToken ct)
    {
        // Validate file size
        if (command.FileStream.Length > MaxFileSizeBytes)
            return Result<string>.Failure(Error.Validation("File size exceeds the maximum allowed size of 5MB."));

        // Validate content type
        if (!AllowedContentTypes.Contains(command.ContentType))
            return Result<string>.Failure(Error.Validation("Only image files (JPEG, PNG, WebP) are allowed."));

        // Build blob name: {branchId}/{timestamp}_{fileName}
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sanitizedFileName = Path.GetFileName(command.FileName);
        var blobName = $"{branchContext.CurrentBranchId}/{timestamp}_{sanitizedFileName}";

        // Upload to Azure Blob Storage
        var blobUrl = await blobService.UploadAsync(
            "clinic-logos", blobName, command.FileStream, command.ContentType);

        return blobUrl;
    }
}
