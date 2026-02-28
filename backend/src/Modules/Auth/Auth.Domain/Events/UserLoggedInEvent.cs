using Shared.Domain;

namespace Auth.Domain.Events;

/// <summary>
/// Published when a user successfully logs in.
/// </summary>
public sealed record UserLoggedInEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
    public DateTime Timestamp { get; init; }
    public string? IpAddress { get; init; }
}
