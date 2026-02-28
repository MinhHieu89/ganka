using Shared.Domain;

namespace Auth.Domain.Entities;

/// <summary>
/// Join entity linking User to Role (many-to-many).
/// A user can have multiple roles, and gets the union of all role permissions.
/// </summary>
public class UserRole : Entity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;

    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = default!;

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
