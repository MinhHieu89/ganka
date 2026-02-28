using Auth.Domain.Enums;
using Shared.Domain;

namespace Auth.Domain.Entities;

/// <summary>
/// Represents a granular permission defined by Module + Action.
/// Permissions are assigned to Roles, and users inherit permissions through role assignments.
/// Unique constraint on (Module, Action) ensures no duplicate permissions.
/// </summary>
public class Permission : Entity
{
    public PermissionModule Module { get; private set; }
    public PermissionAction Action { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() { }

    public Permission(PermissionModule module, PermissionAction action, string description)
    {
        Module = module;
        Action = action;
        Description = description;
    }

    /// <summary>
    /// Returns the permission string in "Module.Action" format (e.g., "Patient.View").
    /// </summary>
    public override string ToString() => $"{Module}.{Action}";
}
