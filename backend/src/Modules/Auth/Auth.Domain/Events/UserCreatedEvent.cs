using Shared.Domain;

namespace Auth.Domain.Events;

/// <summary>
/// Published when a new user is created in the system.
/// </summary>
public sealed record UserCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
}
