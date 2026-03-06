using Auth.Contracts.Queries;

namespace Auth.Application.Features;

/// <summary>
/// Wolverine handler for cross-module VerifyManagerPinQuery from Billing.
/// Stub implementation — always returns valid for non-empty PIN until PIN management is built.
/// </summary>
public static class VerifyManagerPinHandler
{
    public static VerifyManagerPinResponse Handle(VerifyManagerPinQuery query)
    {
        // TODO: Implement actual PIN verification against Auth.Domain.User.ManagerPin
        // For now, accept any non-empty PIN
        return new VerifyManagerPinResponse(!string.IsNullOrWhiteSpace(query.Pin));
    }
}
