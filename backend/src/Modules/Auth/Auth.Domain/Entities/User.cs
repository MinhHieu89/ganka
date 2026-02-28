using Auth.Domain.Events;
using Shared.Domain;

namespace Auth.Domain.Entities;

/// <summary>
/// User aggregate root. Represents an authenticated staff member in the clinic.
/// Implements IAuditable for audit log tracking.
/// Uses DDD patterns: private setters, factory method, domain events.
/// </summary>
public class User : AggregateRoot, IAuditable
{
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PreferredLanguage { get; private set; } = "vi";
    public bool IsActive { get; private set; } = true;

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() { }

    /// <summary>
    /// Factory method for creating a new user.
    /// </summary>
    public static User Create(string email, string fullName, string passwordHash, BranchId branchId)
    {
        var user = new User
        {
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHash
        };

        user.SetBranchId(branchId);
        user.AddDomainEvent(new UserCreatedEvent
        {
            UserId = user.Id,
            Email = email
        });

        return user;
    }

    public void SetLanguagePreference(string language)
    {
        PreferredLanguage = language;
        SetUpdatedAt();
    }

    public void AssignRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return;

        _userRoles.Add(new UserRole(Id, role.Id));
        SetUpdatedAt();
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole is not null)
        {
            _userRoles.Remove(userRole);
            SetUpdatedAt();
        }
    }

    /// <summary>
    /// Returns the union of all permissions from all assigned roles.
    /// </summary>
    public IReadOnlyList<Permission> GetEffectivePermissions()
    {
        return _userRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .DistinctBy(p => p.Id)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Records a successful login event on this user.
    /// </summary>
    public void RecordLogin(string? ipAddress)
    {
        AddDomainEvent(new UserLoggedInEvent
        {
            UserId = Id,
            Email = Email,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress
        });
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void ResetPassword(string newHash)
    {
        PasswordHash = newHash;
        SetUpdatedAt();
    }
}
