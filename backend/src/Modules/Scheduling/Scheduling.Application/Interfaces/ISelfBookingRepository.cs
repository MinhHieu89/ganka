using Scheduling.Domain.Entities;

namespace Scheduling.Application.Interfaces;

/// <summary>
/// Repository interface for self-booking requests.
/// </summary>
public interface ISelfBookingRepository
{
    /// <summary>
    /// Returns all pending self-booking requests for staff review.
    /// </summary>
    Task<List<SelfBookingRequest>> GetPendingAsync(CancellationToken ct = default);

    /// <summary>
    /// Looks up a booking request by its reference number (for public status check).
    /// </summary>
    Task<SelfBookingRequest?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken ct = default);

    /// <summary>
    /// Counts pending requests by phone number (for rate limiting, max 2 per phone).
    /// </summary>
    Task<int> CountPendingByPhoneAsync(string phone, CancellationToken ct = default);

    Task<SelfBookingRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(SelfBookingRequest request);
}
