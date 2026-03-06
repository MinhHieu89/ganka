using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

/// <summary>
/// Repository interface for DispensingRecord persistence operations.
/// Supports prescription dispensing history and duplicate dispensing prevention.
/// </summary>
public interface IDispensingRepository
{
    /// <summary>
    /// Gets a dispensing record by ID (returns domain entity for mutation).
    /// </summary>
    Task<DispensingRecord?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets the dispensing record for a specific prescription, if one exists.
    /// Used to prevent duplicate dispensing of the same prescription.
    /// </summary>
    Task<DispensingRecord?> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken ct);

    /// <summary>
    /// Gets a paginated dispensing history with optional patient filter.
    /// </summary>
    Task<(List<DispensingRecordDto> Items, int TotalCount)> GetHistoryAsync(
        int page,
        int pageSize,
        Guid? patientId,
        CancellationToken ct);

    /// <summary>
    /// Gets pending (not yet dispensed) prescriptions with expiry information.
    /// These are prescriptions from the Clinical module that have no corresponding DispensingRecord.
    /// Used by the pharmacy dispensing queue page.
    /// </summary>
    Task<List<PendingPrescriptionDto>> GetPendingPrescriptionsAsync(Guid? patientId, CancellationToken ct);

    /// <summary>
    /// Adds a new dispensing record to the change tracker.
    /// </summary>
    void Add(DispensingRecord record);
}
