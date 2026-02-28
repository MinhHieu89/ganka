namespace Shared.Domain.Ports;

/// <summary>
/// Marker interface for all external system adapters.
/// Establishes the Anti-Corruption Layer (ACL) adapter pattern:
/// - Domain defines the port (this interface) with domain-shaped request/response
/// - Infrastructure implements the adapter that translates to/from external API models
///
/// Concrete adapter interfaces will be defined per external system in later phases:
/// - IZaloOaAdapter (Phase 3: Notifications)
/// - IMisaAdapter (Phase 6: Billing/Accounting)
/// - ISoYTeAdapter (Phase 7: Regulatory Compliance)
/// </summary>
public interface IExternalSystemAdapter;

/// <summary>
/// Generic external system adapter with typed request/response.
/// Each external integration implements this with domain-specific types,
/// ensuring the domain layer never depends on external API models.
/// </summary>
/// <typeparam name="TRequest">Domain-shaped request type</typeparam>
/// <typeparam name="TResponse">Domain-shaped response type</typeparam>
public interface IExternalSystemAdapter<in TRequest, TResponse> : IExternalSystemAdapter
{
    /// <summary>
    /// Execute an operation against the external system.
    /// Returns a Result to handle external system failures gracefully.
    /// </summary>
    Task<Result<TResponse>> ExecuteAsync(TRequest request, CancellationToken ct = default);
}
