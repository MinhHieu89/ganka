using Auth.Contracts.Dtos;
using Shared.Domain;

namespace Auth.Application.Services;

/// <summary>
/// Permission query service.
/// </summary>
public interface IPermissionService
{
    Task<Result<List<PermissionGroupDto>>> GetPermissionsGroupedByModuleAsync();
}

/// <summary>
/// Permissions grouped by module for the admin UI.
/// </summary>
public sealed record PermissionGroupDto(
    string Module,
    List<PermissionDto> Permissions);
