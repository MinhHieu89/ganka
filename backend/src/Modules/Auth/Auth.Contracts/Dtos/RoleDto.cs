namespace Auth.Contracts.Dtos;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    List<PermissionDto> Permissions);
