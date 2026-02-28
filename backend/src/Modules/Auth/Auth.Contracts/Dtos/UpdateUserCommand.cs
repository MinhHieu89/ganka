namespace Auth.Contracts.Dtos;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FullName,
    bool IsActive,
    List<Guid> RoleIds);
