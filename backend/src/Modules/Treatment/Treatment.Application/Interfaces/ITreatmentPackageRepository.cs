using Treatment.Domain.Entities;

namespace Treatment.Application.Interfaces;

/// <summary>
/// Repository interface for TreatmentPackage aggregate root persistence operations.
/// Provides patient treatment package management with eager loading of child entities,
/// due-soon scheduling queries, and cancellation workflow support.
/// </summary>
public interface ITreatmentPackageRepository
{
    /// <summary>
    /// Gets a treatment package by ID with all child entities eagerly loaded
    /// (Sessions with Consumables, Versions, CancellationRequest).
    /// Returns null if not found.
    /// </summary>
    Task<TreatmentPackage?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all treatment packages for a specific patient, with sessions eagerly loaded.
    /// Ordered by CreatedAt descending (newest first).
    /// </summary>
    Task<List<TreatmentPackage>> GetByPatientIdAsync(Guid patientId, CancellationToken ct);

    /// <summary>
    /// Gets all active treatment packages across all patients, with sessions eagerly loaded.
    /// Ordered by PatientName.
    /// </summary>
    Task<List<TreatmentPackage>> GetActivePackagesAsync(CancellationToken ct);

    /// <summary>
    /// Gets active packages where a session is due soon (last session CompletedAt + MinIntervalDays &lt;= now,
    /// or no sessions completed yet). Used for the "Due Soon" dashboard.
    /// </summary>
    Task<List<TreatmentPackage>> GetDueSoonAsync(CancellationToken ct);

    /// <summary>
    /// Gets all packages with PendingCancellation status, with CancellationRequest eagerly loaded.
    /// Used for the manager approval queue.
    /// </summary>
    Task<List<TreatmentPackage>> GetPendingCancellationsAsync(CancellationToken ct);

    /// <summary>
    /// Adds a new treatment package to the EF Core change tracker.
    /// Call IUnitOfWork.SaveChangesAsync to persist.
    /// </summary>
    void Add(TreatmentPackage package);
}
