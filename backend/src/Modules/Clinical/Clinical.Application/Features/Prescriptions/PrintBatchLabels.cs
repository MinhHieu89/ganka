using Clinical.Application.Interfaces;
using Shared.Domain;

namespace Clinical.Application.Features.Prescriptions;

/// <summary>
/// Query to generate a batch pharmacy label PDF for all items in a prescription.
/// </summary>
public sealed record PrintBatchLabelsQuery(Guid PrescriptionId);

/// <summary>
/// Wolverine handler for generating batch pharmacy labels.
/// Delegates to IDocumentService to generate a multi-page PDF with one label per drug.
/// </summary>
public static class PrintBatchLabelsHandler
{
    public static async Task<Result<byte[]>> Handle(
        PrintBatchLabelsQuery query,
        IDocumentService documentService,
        CancellationToken ct)
    {
        try
        {
            var pdfBytes = await documentService.GenerateBatchPharmacyLabelsAsync(
                query.PrescriptionId, ct);
            return pdfBytes;
        }
        catch (InvalidOperationException)
        {
            return Result<byte[]>.Failure(
                Error.NotFound("DrugPrescription", query.PrescriptionId));
        }
    }
}
