using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Medical image metadata entity. Stores blob reference and metadata for clinical images.
/// NOT a Visit child via aggregate -- images are append-only even after sign-off.
/// This entity bypasses EnsureEditable because diagnostic results (OCT, Meibography)
/// often arrive after the doctor signs the visit record.
/// </summary>
public class MedicalImage : Entity
{
    public Guid VisitId { get; private set; }
    public Guid UploadedById { get; private set; }
    public ImageType Type { get; private set; }
    public EyeTag? EyeTag { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string BlobName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string? Description { get; private set; }

    private MedicalImage() { }

    /// <summary>
    /// Factory method for creating a medical image record with all required fields.
    /// </summary>
    public static MedicalImage Create(
        Guid visitId,
        Guid uploadedById,
        ImageType type,
        EyeTag? eyeTag,
        string originalFileName,
        string blobName,
        string contentType,
        long fileSize,
        string? description = null)
    {
        return new MedicalImage
        {
            VisitId = visitId,
            UploadedById = uploadedById,
            Type = type,
            EyeTag = eyeTag,
            OriginalFileName = originalFileName,
            BlobName = blobName,
            ContentType = contentType,
            FileSize = fileSize,
            Description = description
        };
    }
}
