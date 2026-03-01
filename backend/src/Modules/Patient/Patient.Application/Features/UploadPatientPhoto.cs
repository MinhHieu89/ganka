using Patient.Application.Interfaces;
using Shared.Application.Services;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record UploadPatientPhotoCommand(Guid PatientId, Stream PhotoStream, string FileName);

/// <summary>
/// Wolverine handler for uploading a patient photo to Azure Blob Storage.
/// </summary>
public static class UploadPatientPhotoHandler
{
    public static async Task<Result<string>> Handle(
        UploadPatientPhotoCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IAzureBlobService blobService,
        CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdWithTrackingAsync(command.PatientId, cancellationToken);
        if (patient is null)
            return Result<string>.Failure(Error.NotFound("Patient", command.PatientId));

        var blobName = $"{command.PatientId}/{command.FileName}";
        var contentType = GetContentType(command.FileName);

        var photoUrl = await blobService.UploadAsync("patient-photos", blobName, command.PhotoStream, contentType);

        patient.SetPhotoUrl(photoUrl);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return photoUrl;
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
