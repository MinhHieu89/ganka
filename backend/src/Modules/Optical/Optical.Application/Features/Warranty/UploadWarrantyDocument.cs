using Shared.Domain;

namespace Optical.Application.Features.Warranty;

/// <summary>
/// Command to upload a supporting document for a warranty claim to Azure Blob Storage.
/// Handler implementation provided in plan 08-21.
/// </summary>
public sealed record UploadWarrantyDocumentCommand(Guid ClaimId, Stream FileStream, string FileName);
