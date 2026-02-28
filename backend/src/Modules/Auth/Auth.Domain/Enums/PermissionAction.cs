namespace Auth.Domain.Enums;

/// <summary>
/// Represents actions that can be performed on module resources.
/// Manage implies full control (View + Create + Update + Delete + Export).
/// </summary>
public enum PermissionAction
{
    View,
    Create,
    Update,
    Delete,
    Export,
    Manage
}
