namespace Auth.Contracts.Dtos;

public sealed record CreateRoleCommand(
    string Name,
    string Description,
    List<Guid> PermissionIds);
