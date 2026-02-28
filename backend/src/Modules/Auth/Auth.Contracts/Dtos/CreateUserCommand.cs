namespace Auth.Contracts.Dtos;

public sealed record CreateUserCommand(
    string Email,
    string FullName,
    string Password,
    List<Guid> RoleIds);
