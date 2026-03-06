namespace Pharmacy.Domain.Enums;

/// <summary>
/// Indicates whether a prescription line was successfully dispensed or deliberately skipped.
/// All-or-nothing per drug line: a line is either fully Dispensed or explicitly Skipped.
/// </summary>
public enum DispensingStatus
{
    /// <summary>
    /// The drug line was dispensed to the patient. Batch deductions have been recorded.
    /// </summary>
    Dispensed = 0,

    /// <summary>
    /// The drug line was intentionally skipped (e.g., out of stock, patient refusal, contraindication).
    /// No batch deductions are recorded for skipped lines.
    /// </summary>
    Skipped = 1
}
