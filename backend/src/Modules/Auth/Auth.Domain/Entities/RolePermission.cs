using Shared.Domain;

namespace Auth.Domain.Entities;

/// <summary>
/// Join entity linking Role to Permission (many-to-many).
/// </summary>
public class RolePermission : Entity
{
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = default!;

    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = default!;

    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }
}
