namespace Auth.Contracts.Dtos;

public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    List<Guid> PermissionIds);
