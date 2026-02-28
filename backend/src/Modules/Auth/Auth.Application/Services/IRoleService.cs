using Auth.Contracts.Dtos;
using Shared.Domain;

namespace Auth.Application.Services;

/// <summary>
/// Role management service for admin operations.
/// </summary>
public interface IRoleService
{
    Task<Result<List<RoleDto>>> GetRolesAsync();
    Task<Result<Guid>> CreateRoleAsync(CreateRoleCommand command);
    Task<Result> UpdateRolePermissionsAsync(UpdateRolePermissionsCommand command);
}
