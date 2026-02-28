namespace Auth.Contracts.Dtos;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);
