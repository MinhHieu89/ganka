using Auth.Application.Interfaces;
using Auth.Contracts.Queries;

namespace Auth.Application.Features;

/// <summary>
/// Wolverine handler for cross-module VerifyManagerPinQuery.
/// Verifies the provided PIN against the stored hashed PIN for the manager user.
/// Used by Treatment (approve cancellation) and Billing (approve refund/discount) modules.
/// </summary>
public static class VerifyManagerPinHandler
{
    public static async Task<VerifyManagerPinResponse> HandleAsync(
        VerifyManagerPinQuery query,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Pin))
            return new VerifyManagerPinResponse(false);

        var user = await userRepository.GetByIdAsync(query.ManagerId, ct);
        if (user is null)
            return new VerifyManagerPinResponse(false);

        if (string.IsNullOrWhiteSpace(user.ManagerPinHash))
            return new VerifyManagerPinResponse(false);

        var isValid = passwordHasher.VerifyPassword(query.Pin, user.ManagerPinHash);
        return new VerifyManagerPinResponse(isValid);
    }
}
