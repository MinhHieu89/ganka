namespace Shared.Contracts;

/// <summary>
/// Interface for full data export capability ensuring data ownership.
/// Each module implements this for its own data, allowing Ganka28 clinics
/// to export all their data out of the system at any time.
///
/// This is a legal and business requirement: clinics must have full ownership
/// and portability of their patient data, audit records, and operational data.
/// </summary>
public interface IDataExportService
{
    /// <summary>
    /// Export all data for a specific module to the provided stream.
    /// </summary>
    /// <param name="moduleName">The module to export (e.g., "patient", "clinical", "audit")</param>
    /// <param name="output">The stream to write exported data to</param>
    /// <param name="format">The export format (JSON or CSV)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExportModuleDataAsync(
        string moduleName,
        Stream output,
        ExportFormat format,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Supported data export formats.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// JSON format -- preserves data structure and types.
    /// </summary>
    Json = 0,

    /// <summary>
    /// CSV format -- flat tabular data, compatible with Excel and other tools.
    /// </summary>
    Csv = 1
}
