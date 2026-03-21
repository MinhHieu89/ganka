using Microsoft.Extensions.Logging;
using Shared.Application.Services;

namespace Shared.Infrastructure.Services;

/// <summary>
/// Local file system storage adapter implementing IAzureBlobService for development environments.
/// Stores files under wwwroot/uploads/{containerName}/{blobName} and returns URLs relative to the app root.
/// This avoids the need for Azure Blob Storage or Azurite during local development.
/// </summary>
public sealed class LocalFileStorageService : IAzureBlobService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        ILogger<LocalFileStorageService> logger,
        string basePath,
        string baseUrl)
    {
        _logger = logger;
        _basePath = basePath;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<string> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType)
    {
        var directoryPath = Path.Combine(_basePath, containerName, Path.GetDirectoryName(blobName) ?? "");
        Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(_basePath, containerName, blobName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        // Reset stream position if possible (some handlers read magic bytes first)
        if (content.CanSeek)
            content.Position = 0;
        await content.CopyToAsync(fileStream);

        _logger.LogInformation(
            "Uploaded file {BlobName} to local storage container {ContainerName}",
            blobName, containerName);

        // Return URL path that can be served via UseStaticFiles
        var urlPath = $"{_baseUrl}/uploads/{containerName}/{blobName}";
        return urlPath;
    }

    public Task<Stream> DownloadAsync(string containerName, string blobName)
    {
        var filePath = Path.Combine(_basePath, containerName, blobName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Blob {blobName} not found in container {containerName}");

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task<bool> DeleteAsync(string containerName, string blobName)
    {
        var filePath = Path.Combine(_basePath, containerName, blobName);
        if (!File.Exists(filePath))
            return Task.FromResult(false);

        File.Delete(filePath);
        _logger.LogInformation(
            "Deleted file {BlobName} from local storage container {ContainerName}",
            blobName, containerName);
        return Task.FromResult(true);
    }

    public Task<string> GetSasUrlAsync(string containerName, string blobName, TimeSpan expiry)
    {
        // In local dev, just return the direct URL (no SAS needed)
        var urlPath = $"{_baseUrl}/uploads/{containerName}/{blobName}";
        return Task.FromResult(urlPath);
    }

    public Task<IEnumerable<StorageBlobInfo>> ListBlobsAsync(string containerName, string prefix)
    {
        var containerPath = Path.Combine(_basePath, containerName);
        if (!Directory.Exists(containerPath))
            return Task.FromResult(Enumerable.Empty<StorageBlobInfo>());

        var files = Directory.GetFiles(containerPath, "*", SearchOption.AllDirectories)
            .Where(f =>
            {
                var relativePath = Path.GetRelativePath(containerPath, f).Replace('\\', '/');
                return string.IsNullOrEmpty(prefix) || relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            })
            .Select(f =>
            {
                var info = new FileInfo(f);
                var relativePath = Path.GetRelativePath(containerPath, f).Replace('\\', '/');
                return new StorageBlobInfo(
                    relativePath,
                    GetContentType(info.Extension),
                    info.Length,
                    new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero));
            });

        return Task.FromResult(files);
    }

    private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };
}
