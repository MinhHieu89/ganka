using System.Reflection;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Shared.Domain;

namespace Auth.Unit.Tests;

/// <summary>
/// Helper methods for constructing domain entities in tests.
/// Uses reflection to set navigation properties since domain entities use private setters.
/// </summary>
internal static class TestHelpers
{
    private static readonly BranchId DefaultBranchId =
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public static User CreateUser(
        string email = "test@test.com",
        string fullName = "Test User",
        string passwordHash = "hashed-password",
        bool isActive = true,
        BranchId? branchId = null)
    {
        var user = User.Create(email, fullName, passwordHash, branchId ?? DefaultBranchId);
        if (!isActive) user.Deactivate();
        return user;
    }

    public static User CreateUserWithRoles(
        string email = "test@test.com",
        string fullName = "Test User",
        string passwordHash = "hashed-password",
        bool isActive = true,
        params Role[] roles)
    {
        var user = CreateUser(email, fullName, passwordHash, isActive);
        foreach (var role in roles)
            user.AssignRole(role);
        return user;
    }

    public static Role CreateRole(
        string name = "TestRole",
        string description = "Test Role Description",
        bool isSystem = false,
        params Permission[] permissions)
    {
        var role = new Role(name, description, isSystem, DefaultBranchId);
        if (permissions.Length > 0)
            role.UpdatePermissions(permissions.ToList());
        return role;
    }

    public static Permission CreatePermission(
        PermissionModule module = PermissionModule.Auth,
        PermissionAction action = PermissionAction.View,
        string description = "Test Permission")
    {
        return new Permission(module, action, description);
    }

    public static RefreshToken CreateRefreshToken(
        string token = "test-refresh-token",
        Guid? userId = null,
        DateTime? expiresAt = null,
        Guid? familyId = null,
        bool isRevoked = false)
    {
        var rt = new RefreshToken(
            token,
            userId ?? Guid.NewGuid(),
            expiresAt ?? DateTime.UtcNow.AddDays(7),
            familyId ?? Guid.NewGuid());

        if (isRevoked) rt.Revoke();
        return rt;
    }

    /// <summary>
    /// Creates a RefreshToken with User navigation property set via reflection.
    /// Required for handlers that access existingToken.User.
    /// </summary>
    public static RefreshToken CreateRefreshTokenWithUser(
        User user,
        string token = "test-refresh-token",
        DateTime? expiresAt = null,
        Guid? familyId = null,
        bool isRevoked = false)
    {
        var rt = new RefreshToken(
            token,
            user.Id,
            expiresAt ?? DateTime.UtcNow.AddDays(7),
            familyId ?? Guid.NewGuid());

        if (isRevoked) rt.Revoke();

        // Set User navigation property via reflection
        var userProp = typeof(RefreshToken).GetProperty("User")!;
        userProp.SetValue(rt, user);

        return rt;
    }

    /// <summary>
    /// Sets the Role navigation property on a UserRole via reflection.
    /// Used when handlers access user.UserRoles.Select(ur => ur.Role.Name).
    /// </summary>
    public static void SetUserRoleNavigation(User user, Role role)
    {
        // The UserRole has been created by user.AssignRole(role), but the navigation
        // property Role is not set since we're not using EF. Set it via reflection.
        var userRolesField = typeof(User).GetField("_userRoles", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var userRoles = (List<UserRole>)userRolesField.GetValue(user)!;
        var userRole = userRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
        if (userRole is not null)
        {
            var roleProp = typeof(UserRole).GetProperty("Role")!;
            roleProp.SetValue(userRole, role);
        }
    }

    /// <summary>
    /// Sets the Permission navigation property on a RolePermission via reflection.
    /// Used when handlers access role.RolePermissions.Select(rp => rp.Permission).
    /// </summary>
    public static void SetRolePermissionNavigation(Role role, Permission permission)
    {
        var rpField = typeof(Role).GetField("_rolePermissions", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var rolePermissions = (List<RolePermission>)rpField.GetValue(role)!;
        var rp = rolePermissions.FirstOrDefault(x => x.PermissionId == permission.Id);
        if (rp is not null)
        {
            var permProp = typeof(RolePermission).GetProperty("Permission")!;
            permProp.SetValue(rp, permission);
        }
    }

    /// <summary>
    /// Creates a user with roles and all navigation properties properly wired up.
    /// Useful for handlers that traverse user.UserRoles[].Role.RolePermissions[].Permission.
    /// </summary>
    public static User CreateFullyWiredUser(
        string email = "test@test.com",
        string fullName = "Test User",
        string passwordHash = "hashed-password",
        bool isActive = true,
        params (Role role, Permission[] permissions)[] rolePermissions)
    {
        var user = CreateUser(email, fullName, passwordHash, isActive);
        foreach (var (role, permissions) in rolePermissions)
        {
            if (permissions.Length > 0)
                role.UpdatePermissions(permissions.ToList());

            foreach (var perm in permissions)
                SetRolePermissionNavigation(role, perm);

            user.AssignRole(role);
            SetUserRoleNavigation(user, role);
        }
        return user;
    }
}
