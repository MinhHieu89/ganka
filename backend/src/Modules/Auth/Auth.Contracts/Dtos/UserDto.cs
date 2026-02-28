namespace Auth.Contracts.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FullName,
    string PreferredLanguage,
    bool IsActive,
    List<string> Roles,
    List<string> Permissions);
