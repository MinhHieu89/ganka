namespace Auth.Contracts.Dtos;

public sealed record PermissionDto(
    Guid Id,
    string Module,
    string Action,
    string Description);
