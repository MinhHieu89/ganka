namespace Auth.Contracts.Dtos;

public sealed record AssignRolesCommand(
    Guid UserId,
    List<Guid> RoleIds);
