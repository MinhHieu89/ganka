namespace Shared.Application.Services;

/// <summary>
/// Port (interface) for Azure Blob Storage operations.
/// Defined in Application layer per DDD -- the adapter (implementation) lives in Infrastructure.
/// Used for storing medical images, documents, and other binary data.
/// </summary>
public interface IAzureBlobService
{
    /// <summary>
    /// Upload a blob to the specified container.
    /// </summary>
    /// <param name="containerName">Container name (e.g., "clinical-images", "documents")</param>
    /// <param name="blobName">Blob name (e.g., "{entityId}/{timestamp}_{filename}")</param>
    /// <param name="content">The content stream to upload</param>
    /// <param name="contentType">MIME type (e.g., "image/jpeg", "application/pdf")</param>
    /// <returns>The URL of the uploaded blob</returns>
    Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType);

    /// <summary>
    /// Download a blob from the specified container.
    /// </summary>
    /// <returns>Stream containing the blob data</returns>
    Task<Stream> DownloadAsync(string containerName, string blobName);

    /// <summary>
    /// Delete a blob from the specified container.
    /// </summary>
    /// <returns>True if the blob was deleted, false if it did not exist</returns>
    Task<bool> DeleteAsync(string containerName, string blobName);

    /// <summary>
    /// Generate a time-limited SAS URL for direct client access to a blob.
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name</param>
    /// <param name="expiry">How long the SAS URL remains valid</param>
    /// <returns>SAS URL with read permission</returns>
    Task<string> GetSasUrlAsync(string containerName, string blobName, TimeSpan expiry);

    /// <summary>
    /// List blobs in a container matching the given prefix.
    /// </summary>
    /// <returns>Collection of blob metadata</returns>
    Task<IEnumerable<StorageBlobInfo>> ListBlobsAsync(string containerName, string prefix);
}

/// <summary>
/// Metadata about a blob in storage.
/// </summary>
public sealed record StorageBlobInfo(
    string Name,
    string ContentType,
    long Size,
    DateTimeOffset? LastModified);
