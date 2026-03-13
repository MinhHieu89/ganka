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
        if (string.IsNullOrWhiteSpace(command.FileName))
            return Result.Failure<string>(Error.Validation("File name is required."));

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

    private static readonly HashSet<string> AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException($"File type '{ext}' is not allowed. Only PDF, JPG, and PNG files are accepted.");

        return ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}
