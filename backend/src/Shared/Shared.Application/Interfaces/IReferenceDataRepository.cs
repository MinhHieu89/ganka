using Shared.Domain;

namespace Shared.Application.Interfaces;

/// <summary>
/// Repository interface for cross-module ICD-10 reference data.
/// Application layer depends on this interface; Infrastructure provides the implementation.
/// </summary>
public interface IReferenceDataRepository
{
    /// <summary>
    /// Searches ICD-10 codes by code, English description, or Vietnamese description.
    /// Results ordered by code, limited to specified count.
    /// </summary>
    Task<List<Icd10Code>> SearchAsync(string term, int limit = 50, CancellationToken ct = default);

    /// <summary>
    /// Gets ICD-10 codes by their code values (for doctor favorites enrichment).
    /// Results ordered by code.
    /// </summary>
    Task<List<Icd10Code>> GetByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default);
}
