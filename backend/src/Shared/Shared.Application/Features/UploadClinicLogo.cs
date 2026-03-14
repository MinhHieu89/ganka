using Shared.Application.Interfaces;
using Shared.Application.Services;
using Shared.Domain;

namespace Shared.Application.Features;

/// <summary>
/// Command to upload a clinic logo image to Azure Blob Storage.
/// FileSize is passed from the endpoint (file.Length) to avoid calling Stream.Length in the handler.
/// </summary>
public sealed record UploadClinicLogoCommand(
    Stream FileStream,
    string ContentType,
    string FileName,
    long FileSize);

/// <summary>
/// Wolverine handler for uploading a clinic logo.
/// Validates file size (max 5MB), content type (image/jpeg, image/png, image/webp),
/// magic bytes, uploads to Azure Blob Storage, and persists LogoBlobUrl to ClinicSettings.
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

    // Magic byte signatures for image validation
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngMagic = [0x89, 0x50, 0x4E, 0x47];
    private static readonly byte[] RiffMagic = [0x52, 0x49, 0x46, 0x46]; // RIFF header for WebP
    private static readonly byte[] WebpSignature = [(byte)'W', (byte)'E', (byte)'B', (byte)'P'];

    public static async Task<Result<string>> Handle(
        UploadClinicLogoCommand command,
        IAzureBlobService blobService,
        IClinicSettingsService settingsService,
        IBranchContext branchContext,
        CancellationToken ct)
    {
        // Validate file size using FileSize from endpoint (not Stream.Length)
        if (command.FileSize > MaxFileSizeBytes)
            return Result<string>.Failure(Error.Validation("File size exceeds the maximum allowed size of 5MB."));

        // Validate content type
        if (!AllowedContentTypes.Contains(command.ContentType))
            return Result<string>.Failure(Error.Validation("Only image files (JPEG, PNG, WebP) are allowed."));

        // Validate magic bytes
        var magicBytesValid = await ValidateMagicBytesAsync(command.FileStream, command.ContentType);
        if (!magicBytesValid)
            return Result<string>.Failure(Error.Validation(
                "File content does not match declared content type. The file magic bytes do not correspond to the expected image format."));

        // Build blob name: {branchId}/{timestamp}_{fileName}
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sanitizedFileName = Path.GetFileName(command.FileName);
        var blobName = $"{branchContext.CurrentBranchId}/{timestamp}_{sanitizedFileName}";

        // Upload to Azure Blob Storage
        var blobUrl = await blobService.UploadAsync(
            "clinic-logos", blobName, command.FileStream, command.ContentType);

        // Persist the blob URL to ClinicSettings
        await settingsService.UpdateLogoUrlAsync(blobUrl, ct);

        return blobUrl;
    }

    /// <summary>
    /// Validates that the file's magic bytes match the declared content type.
    /// Reads first 12 bytes, checks signature, then rewinds the stream.
    /// </summary>
    private static async Task<bool> ValidateMagicBytesAsync(Stream stream, string contentType)
    {
        var header = new byte[12];
        var bytesRead = await stream.ReadAsync(header.AsMemory(0, 12));

        // Rewind stream for subsequent processing
        if (stream.CanSeek)
            stream.Position = 0;

        if (bytesRead < 4)
            return false;

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => header[0] == JpegMagic[0] && header[1] == JpegMagic[1] && header[2] == JpegMagic[2],
            "image/png" => header[0] == PngMagic[0] && header[1] == PngMagic[1] && header[2] == PngMagic[2] && header[3] == PngMagic[3],
            "image/webp" => bytesRead >= 12
                && header[0] == RiffMagic[0] && header[1] == RiffMagic[1] && header[2] == RiffMagic[2] && header[3] == RiffMagic[3]
                && header[8] == WebpSignature[0] && header[9] == WebpSignature[1] && header[10] == WebpSignature[2] && header[11] == WebpSignature[3],
            _ => false
        };
    }
}
