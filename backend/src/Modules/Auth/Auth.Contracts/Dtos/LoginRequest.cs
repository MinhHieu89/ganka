namespace Auth.Contracts.Dtos;

public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe = false);
