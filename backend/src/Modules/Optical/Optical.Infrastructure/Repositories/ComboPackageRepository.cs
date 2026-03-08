using Microsoft.EntityFrameworkCore;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;

namespace Optical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IComboPackageRepository"/>.
/// Provides data access for ComboPackage preset frame+lens bundles.
/// Supports active-only filtering for order creation dropdowns and admin listing with inactive packages.
/// </summary>
public sealed class ComboPackageRepository(OpticalDbContext context) : IComboPackageRepository
{
    /// <summary>
    /// Gets a combo package by ID.
    /// Returns null if not found.
    /// </summary>
    public async Task<ComboPackage?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.ComboPackages
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Gets all combo packages, optionally including inactive ones.
    /// Ordered by Name for consistent dropdown display.
    /// Active-only filter is the default (includeInactive = false) for order creation.
    /// </summary>
    public async Task<List<ComboPackage>> GetAllAsync(bool includeInactive, CancellationToken ct)
    {
        var query = context.ComboPackages
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Adds a new combo package to the EF Core change tracker.
    /// </summary>
    public void Add(ComboPackage package)
    {
        context.ComboPackages.Add(package);
    }
}
