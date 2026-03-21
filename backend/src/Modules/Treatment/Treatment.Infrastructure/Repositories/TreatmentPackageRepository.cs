using Microsoft.EntityFrameworkCore;
using Treatment.Application.Interfaces;
using Treatment.Domain.Entities;
using Treatment.Domain.Enums;

namespace Treatment.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ITreatmentPackageRepository"/>.
/// Provides data access for the TreatmentPackage aggregate root including its child collections
/// (Sessions with Consumables, Versions, CancellationRequest).
/// Uses AsSplitQuery on GetByIdAsync to prevent cartesian explosion with multiple Includes.
/// GetDueSoonAsync uses client-side filtering to calculate session due dates.
/// </summary>
public sealed class TreatmentPackageRepository(TreatmentDbContext context) : ITreatmentPackageRepository
{
    /// <summary>
    /// Gets a treatment package by ID with all child entities eagerly loaded.
    /// Uses AsSplitQuery() to avoid cartesian explosion with multiple child collections.
    /// Returns null if not found.
    /// </summary>
    public async Task<TreatmentPackage?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.TreatmentPackages
            .Include(x => x.Sessions)
                .ThenInclude(s => s.Consumables)
            .Include(x => x.Versions)
            .Include(x => x.CancellationRequest)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Gets all treatment packages for a specific patient, with sessions eagerly loaded.
    /// Ordered by CreatedAt descending (newest first).
    /// </summary>
    public async Task<List<TreatmentPackage>> GetByPatientIdAsync(Guid patientId, CancellationToken ct)
    {
        return await context.TreatmentPackages
            .Include(x => x.Sessions)
            .AsNoTracking()
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets all active treatment packages across all patients, with sessions eagerly loaded.
    /// Ordered by PatientName for display in the management dashboard.
    /// </summary>
    public async Task<List<TreatmentPackage>> GetActivePackagesAsync(CancellationToken ct)
    {
        return await context.TreatmentPackages
            .Include(x => x.Sessions)
            .AsNoTracking()
            .Where(x => x.Status == PackageStatus.Active
                     || x.Status == PackageStatus.Paused
                     || x.Status == PackageStatus.PendingCancellation)
            .OrderBy(x => x.PatientName)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets active packages where a session is due.
    /// A session is due when:
    ///   (a) no sessions have been completed yet, OR
    ///   (b) the last completed session's CompletedAt + MinIntervalDays &lt;= DateTime.UtcNow
    /// Uses client-side filtering after loading active packages with sessions,
    /// because the due-date calculation requires per-session data.
    /// </summary>
    public async Task<List<TreatmentPackage>> GetDueSoonAsync(CancellationToken ct)
    {
        var utcNow = DateTime.UtcNow;

        // Load all active packages with their sessions
        var activePackages = await context.TreatmentPackages
            .Include(x => x.Sessions)
            .AsNoTracking()
            .Where(x => x.Status == PackageStatus.Active)
            .ToListAsync(ct);

        // Client-side filter: packages where a session is due
        return activePackages
            .Where(p =>
            {
                // Package must have remaining sessions
                if (p.SessionsRemaining <= 0)
                    return false;

                var completedSessions = p.Sessions
                    .Where(s => s.Status == SessionStatus.Completed && s.CompletedAt.HasValue)
                    .ToList();

                // (a) No sessions completed yet — session is immediately due
                if (completedSessions.Count == 0)
                    return true;

                // (b) Last completed session + MinIntervalDays <= now
                var lastCompleted = completedSessions
                    .OrderByDescending(s => s.CompletedAt)
                    .First();

                return lastCompleted.CompletedAt!.Value.AddDays(p.MinIntervalDays) <= utcNow;
            })
            .OrderBy(p => p.PatientName)
            .ToList();
    }

    /// <summary>
    /// Gets all packages with PendingCancellation status, with CancellationRequest eagerly loaded.
    /// Used for the manager approval queue.
    /// </summary>
    public async Task<List<TreatmentPackage>> GetPendingCancellationsAsync(CancellationToken ct)
    {
        return await context.TreatmentPackages
            .Include(x => x.CancellationRequest)
            .AsNoTracking()
            .Where(x => x.Status == PackageStatus.PendingCancellation)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Adds a new treatment package to the EF Core change tracker.
    /// </summary>
    public void Add(TreatmentPackage package)
    {
        context.TreatmentPackages.Add(package);
    }
}
