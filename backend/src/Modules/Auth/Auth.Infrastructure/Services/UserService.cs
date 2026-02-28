using Auth.Application.Services;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Auth.Infrastructure.Services;

/// <summary>
/// User management service implementation for admin operations.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly AuthDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;

    public UserService(AuthDbContext dbContext, PasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<(List<UserDto> Users, int TotalCount)>> GetUsersAsync(int page, int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        var skip = (page - 1) * pageSize;

        var totalCount = await _dbContext.Users.CountAsync();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .OrderBy(u => u.FullName)
            .Skip(skip)
            .Take(pageSize)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.FullName,
                u.PreferredLanguage,
                u.IsActive,
                u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                u.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => $"{rp.Permission.Module}.{rp.Permission.Action}")
                    .Distinct()
                    .ToList()))
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<Result<Guid>> CreateUserAsync(CreateUserCommand command)
    {
        var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == command.Email);
        if (emailExists)
            return Result<Guid>.Failure(Error.Conflict("A user with this email already exists."));

        var roles = await _dbContext.Roles
            .Where(r => command.RoleIds.Contains(r.Id))
            .ToListAsync();

        if (roles.Count != command.RoleIds.Count)
            return Result<Guid>.Failure(Error.Validation("One or more role IDs are invalid."));

        // Use default branch for now
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var passwordHash = _passwordHasher.HashPassword(command.Password);
        var user = User.Create(command.Email, command.FullName, passwordHash, branchId);

        foreach (var role in roles)
            user.AssignRole(role);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user.Id;
    }

    public async Task<Result> UpdateUserAsync(Guid userId, UpdateUserCommand command)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Result.Failure(Error.NotFound("User", userId));

        if (!command.IsActive && user.IsActive)
            user.Deactivate();

        // Update roles
        var currentRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var newRoles = await _dbContext.Roles
            .Where(r => command.RoleIds.Contains(r.Id))
            .ToListAsync();

        foreach (var roleId in currentRoleIds.Where(r => !command.RoleIds.Contains(r)))
            user.RemoveRole(roleId);

        foreach (var role in newRoles.Where(r => !currentRoleIds.Contains(r.Id)))
            user.AssignRole(role);

        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AssignRolesAsync(Guid userId, AssignRolesCommand command)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Result.Failure(Error.NotFound("User", userId));

        var roles = await _dbContext.Roles
            .Where(r => command.RoleIds.Contains(r.Id))
            .ToListAsync();

        var existingRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        foreach (var roleId in existingRoleIds)
            user.RemoveRole(roleId);

        foreach (var role in roles)
            user.AssignRole(role);

        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
}
