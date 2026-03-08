using Microsoft.EntityFrameworkCore;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Optical.Domain.Enums;

namespace Optical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IFrameRepository"/>.
/// Provides frame catalog management including multi-field search with enum filtering,
/// EAN-13 barcode lookup, uniqueness validation, and sequence number generation.
/// </summary>
public sealed class FrameRepository : IFrameRepository
{
    private readonly OpticalDbContext _context;

    public FrameRepository(OpticalDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<Frame?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Frames.FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <inheritdoc />
    public async Task<Frame?> GetByBarcodeAsync(string barcode, CancellationToken ct)
        => await _context.Frames.FirstOrDefaultAsync(x => x.Barcode == barcode, ct);

    /// <inheritdoc />
    public async Task<List<Frame>> GetAllAsync(bool includeInactive, CancellationToken ct)
    {
        return await _context.Frames
            .AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Brand)
            .ThenBy(x => x.Model)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<List<Frame>> SearchAsync(
        string? searchTerm,
        int? material,
        int? frameType,
        int? gender,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = BuildSearchQuery(searchTerm, material, frameType, gender);

        return await query
            .AsNoTracking()
            .OrderBy(x => x.Brand)
            .ThenBy(x => x.Model)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync(
        string? searchTerm,
        int? material,
        int? frameType,
        int? gender,
        CancellationToken ct)
    {
        var query = BuildSearchQuery(searchTerm, material, frameType, gender);
        return await query.CountAsync(ct);
    }

    /// <inheritdoc />
    public async Task<long> GetNextSequenceNumberAsync(CancellationToken ct)
    {
        // Simple approach: total frame count + 1.
        // This is used as the sequence component in clinic-prefix EAN-13 generation.
        // Not guaranteed to be globally unique but provides a monotonically increasing
        // value suitable for barcode suffix generation when combined with the clinic prefix.
        var count = await _context.Frames.CountAsync(ct);
        return count + 1;
    }

    /// <inheritdoc />
    public async Task<bool> IsBarcodeUniqueAsync(string barcode, Guid? excludeId, CancellationToken ct)
    {
        return !await _context.Frames
            .AnyAsync(x => x.Barcode == barcode && (excludeId == null || x.Id != excludeId), ct);
    }

    /// <inheritdoc />
    public void Add(Frame frame) => _context.Frames.Add(frame);

    // --- Private helpers ---

    /// <summary>
    /// Builds the base IQueryable for both SearchAsync and GetTotalCountAsync,
    /// applying identical filters to ensure consistent pagination results.
    /// Only returns active frames; applies text search and enum filters conditionally.
    /// </summary>
    private IQueryable<Frame> BuildSearchQuery(
        string? searchTerm,
        int? material,
        int? frameType,
        int? gender)
    {
        // Start with active frames only (catalog search should not return soft-deleted items)
        IQueryable<Frame> query = _context.Frames.Where(x => x.IsActive);

        // Text search across Brand, Model, Color, and Barcode — EF Core translates Contains to SQL LIKE
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x =>
                x.Brand.Contains(searchTerm) ||
                x.Model.Contains(searchTerm) ||
                x.Color.Contains(searchTerm) ||
                (x.Barcode != null && x.Barcode.Contains(searchTerm)));
        }

        // Enum filters — cast int? to enum for EF Core translation
        if (material.HasValue)
        {
            var materialEnum = (FrameMaterial)material.Value;
            query = query.Where(x => x.Material == materialEnum);
        }

        if (frameType.HasValue)
        {
            var frameTypeEnum = (FrameType)frameType.Value;
            query = query.Where(x => x.Type == frameTypeEnum);
        }

        if (gender.HasValue)
        {
            var genderEnum = (FrameGender)gender.Value;
            query = query.Where(x => x.Gender == genderEnum);
        }

        return query;
    }
}
