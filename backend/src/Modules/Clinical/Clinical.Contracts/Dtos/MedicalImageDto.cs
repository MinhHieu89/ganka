namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for medical image metadata with SAS URL for secure access.
/// </summary>
public sealed record MedicalImageDto(
    Guid Id,
    Guid VisitId,
    int Type,
    int? EyeTag,
    string FileName,
    string Url,
    string ContentType,
    long FileSize,
    string? Description,
    DateTime CreatedAt);

/// <summary>
/// Command to upload a medical image for a visit.
/// Uses Stream for the file content (multipart upload).
/// </summary>
public sealed record UploadMedicalImageCommand(
    Guid VisitId,
    Stream Stream,
    string FileName,
    string ContentType,
    long FileSize,
    int ImageType,
    int? EyeTag);

/// <summary>
/// Query to compare images of the same type across two visits.
/// </summary>
public sealed record GetImageComparisonQuery(
    Guid PatientId,
    Guid VisitId1,
    Guid VisitId2,
    int ImageType);

/// <summary>
/// Response containing images from two visits for side-by-side comparison.
/// </summary>
public sealed record ImageComparisonResponse(
    MedicalImageDto[] Visit1Images,
    MedicalImageDto[] Visit2Images);
