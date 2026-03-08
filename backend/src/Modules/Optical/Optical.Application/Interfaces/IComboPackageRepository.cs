using Optical.Domain.Entities;

namespace Optical.Application.Interfaces;

/// <summary>
/// Repository interface for ComboPackage persistence operations.
/// Supports admin creation of preset frame+lens combo packages and active-only filtering for order creation.
/// </summary>
public interface IComboPackageRepository
{
    /// <summary>
    /// Gets a combo package by ID (returns domain entity for mutation).
    /// </summary>
    Task<ComboPackage?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all combo packages.
    /// </summary>
    /// <param name="includeInactive">When true, returns both active and inactive packages.
    /// When false (default), returns only active packages available for order selection.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<List<ComboPackage>> GetAllAsync(bool includeInactive, CancellationToken ct);

    /// <summary>
    /// Adds a new combo package to the EF Core change tracker.
    /// The caller must invoke IUnitOfWork.SaveChangesAsync to persist.
    /// </summary>
    void Add(ComboPackage package);
}
