using Shared.Domain;

namespace Auth.Domain.Entities;

/// <summary>
/// Represents a role in the RBAC system. Roles contain permissions and are assigned to users.
/// System roles (IsSystem=true) are predefined and cannot be deleted:
///   Admin, Doctor, Technician, Nurse, Cashier, OpticalStaff, Manager, Accountant
/// Custom roles can be created by administrators.
/// </summary>
public class Role : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private Role() { }

    public Role(string name, string description, bool isSystem, BranchId branchId)
    {
        Name = name;
        Description = description;
        IsSystem = isSystem;
        SetBranchId(branchId);
    }

    /// <summary>
    /// Replace all permissions on this role with the provided set.
    /// </summary>
    public void UpdatePermissions(List<Permission> permissions)
    {
        _rolePermissions.Clear();
        foreach (var permission in permissions)
        {
            _rolePermissions.Add(new RolePermission(Id, permission.Id));
        }
        SetUpdatedAt();
    }

    /// <summary>
    /// Add a single permission to this role.
    /// </summary>
    public void AddPermission(Permission permission)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permission.Id))
            return;

        _rolePermissions.Add(new RolePermission(Id, permission.Id));
        SetUpdatedAt();
    }

    /// <summary>
    /// Remove a permission from this role by permission ID.
    /// </summary>
    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission is not null)
        {
            _rolePermissions.Remove(rolePermission);
            SetUpdatedAt();
        }
    }
}
