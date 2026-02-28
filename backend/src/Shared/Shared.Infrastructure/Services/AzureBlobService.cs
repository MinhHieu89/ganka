using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Application.Services;

namespace Shared.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage adapter implementing IAzureBlobService.
/// Uses Azure.Storage.Blobs SDK for all blob operations.
/// Connection string from configuration: "AzureStorage:ConnectionString".
/// Container naming: one container per module (e.g., "clinical-images", "documents").
/// Blob naming convention: {entityId}/{timestamp}_{filename}.
/// </summary>
public sealed class AzureBlobService : IAzureBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobService> _logger;

    public AzureBlobService(IConfiguration configuration, ILogger<AzureBlobService> logger)
    {
        _logger = logger;

        var connectionString = configuration["AzureStorage:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("AzureStorage:ConnectionString not configured. Blob operations will fail at runtime.");
            // Create with a dummy connection string -- service will fail gracefully at operation time
            // This allows the application to start without Azure Storage configured (development)
            connectionString = "UseDevelopmentStorage=true";
        }

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobClient = containerClient.GetBlobClient(blobName);

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        await blobClient.UploadAsync(content, options);

        _logger.LogInformation("Uploaded blob {BlobName} to container {ContainerName}", blobName, containerName);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task<bool> DeleteAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

        if (response.Value)
            _logger.LogInformation("Deleted blob {BlobName} from container {ContainerName}", blobName, containerName);

        return response.Value;
    }

    public Task<string> GetSasUrlAsync(string containerName, string blobName, TimeSpan expiry)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            _logger.LogWarning(
                "Cannot generate SAS URI for blob {BlobName}. Ensure the connection uses account key.",
                blobName);
            return Task.FromResult(blobClient.Uri.ToString());
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b", // blob
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // clock skew tolerance
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return Task.FromResult(sasUri.ToString());
    }

    public async Task<IEnumerable<StorageBlobInfo>> ListBlobsAsync(string containerName, string prefix)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobs = new List<StorageBlobInfo>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, prefix, CancellationToken.None))
        {
            blobs.Add(new StorageBlobInfo(
                blobItem.Name,
                blobItem.Properties.ContentType ?? "application/octet-stream",
                blobItem.Properties.ContentLength ?? 0,
                blobItem.Properties.LastModified));
        }

        return blobs;
    }
}
