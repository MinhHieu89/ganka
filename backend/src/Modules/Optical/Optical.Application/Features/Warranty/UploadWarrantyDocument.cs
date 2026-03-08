using Optical.Application.Interfaces;
using Shared.Application.Services;
using Shared.Domain;

namespace Optical.Application.Features.Warranty;

/// <summary>
/// Command to upload a supporting document for a warranty claim to Azure Blob Storage.
/// </summary>
public sealed record UploadWarrantyDocumentCommand(Guid ClaimId, Stream FileStream, string FileName);

/// <summary>
/// Wolverine static handler for uploading warranty claim supporting documents.
/// Uploads the file to Azure Blob Storage in the "warranty-documents" container,
/// then adds the returned URL to the warranty claim's DocumentUrls collection.
/// </summary>
public static class UploadWarrantyDocumentHandler
{
    public static async Task<Result<string>> Handle(
        UploadWarrantyDocumentCommand command,
        IWarrantyClaimRepository repository,
        IAzureBlobService blobService,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var claim = await repository.GetByIdAsync(command.ClaimId, ct);
        if (claim is null)
            return Result.Failure<string>(Error.NotFound("WarrantyClaim", command.ClaimId));

        // Build a unique blob name: claimId/timestamp_filename
        var sanitizedFileName = SanitizeFileName(command.FileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var blobName = $"{command.ClaimId}/{timestamp}_{sanitizedFileName}";

        var contentType = GetContentType(command.FileName);

        var url = await blobService.UploadAsync(
            containerName: "warranty-documents",
            blobName: blobName,
            content: command.FileStream,
            contentType: contentType);

        claim.AddDocumentUrl(url);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(url);
    }

    private static string SanitizeFileName(string fileName) =>
        string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
