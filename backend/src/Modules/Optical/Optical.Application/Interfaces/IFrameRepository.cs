using Optical.Domain.Entities;

namespace Optical.Application.Interfaces;

/// <summary>
/// Repository interface for Frame inventory persistence operations.
/// Provides frame catalog management, search with filtering, barcode lookup,
/// and sequence number generation for EAN-13 barcode assignment.
/// </summary>
public interface IFrameRepository
{
    /// <summary>
    /// Gets a frame by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    Task<Frame?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets a frame by its EAN-13 barcode.
    /// Returns null if no frame has the specified barcode.
    /// </summary>
    Task<Frame?> GetByBarcodeAsync(string barcode, CancellationToken ct);

    /// <summary>
    /// Gets all frames, optionally including inactive (soft-deleted) entries.
    /// Ordered by Brand, then Model.
    /// </summary>
    Task<List<Frame>> GetAllAsync(bool includeInactive, CancellationToken ct);

    /// <summary>
    /// Searches frames with optional text search across Brand, Model, Color, Barcode,
    /// and optional enum filters for Material, FrameType, and Gender.
    /// Supports pagination via page/pageSize.
    /// Results ordered by Brand, then Model.
    /// </summary>
    Task<List<Frame>> SearchAsync(
        string? searchTerm,
        int? material,
        int? frameType,
        int? gender,
        int page,
        int pageSize,
        CancellationToken ct);

    /// <summary>
    /// Returns the total count of frames matching the same search criteria as SearchAsync,
    /// used for pagination metadata.
    /// </summary>
    Task<int> GetTotalCountAsync(
        string? searchTerm,
        int? material,
        int? frameType,
        int? gender,
        CancellationToken ct);

    /// <summary>
    /// Returns the next sequence number for EAN-13 barcode generation.
    /// Uses the total frame count + 1 as a simple incrementing sequence.
    /// </summary>
    Task<long> GetNextSequenceNumberAsync(CancellationToken ct);

    /// <summary>
    /// Returns true if no frame (other than the one with <paramref name="excludeId"/>) has the given barcode.
    /// Used for barcode uniqueness validation on create (excludeId = null) and update (excludeId = frame's own Id).
    /// </summary>
    Task<bool> IsBarcodeUniqueAsync(string barcode, Guid? excludeId, CancellationToken ct);

    /// <summary>
    /// Adds a new frame to the EF Core change tracker.
    /// Call IUnitOfWork.SaveChangesAsync to persist.
    /// </summary>
    void Add(Frame frame);
}
