namespace Clinical.Application.Interfaces;

/// <summary>
/// Service interface for generating clinical document PDFs.
/// Each method loads relevant data and returns the PDF as a byte array.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Generates a drug prescription PDF (A5 paper, MOH-compliant layout).
    /// </summary>
    Task<byte[]> GenerateDrugPrescriptionAsync(Guid visitId, CancellationToken ct);

    /// <summary>
    /// Generates an optical prescription (glasses Rx) PDF (A4 paper).
    /// </summary>
    Task<byte[]> GenerateOpticalPrescriptionAsync(Guid visitId, CancellationToken ct);

    /// <summary>
    /// Generates a referral letter PDF (A4 paper).
    /// </summary>
    Task<byte[]> GenerateReferralLetterAsync(Guid visitId, string referralReason, string referralTo, CancellationToken ct);

    /// <summary>
    /// Generates a consent form PDF (A4 paper).
    /// </summary>
    Task<byte[]> GenerateConsentFormAsync(Guid visitId, string procedureType, CancellationToken ct);

    /// <summary>
    /// Generates a pharmacy label PDF (small label ~70x35mm).
    /// </summary>
    Task<byte[]> GeneratePharmacyLabelAsync(Guid prescriptionItemId, CancellationToken ct);

    /// <summary>
    /// Generates a batch pharmacy label PDF with one label per drug in a prescription.
    /// Each page is a 70x35mm label for thermal printer output.
    /// </summary>
    Task<byte[]> GenerateBatchPharmacyLabelsAsync(Guid prescriptionId, CancellationToken ct);
}
