using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Shared.Domain;

namespace Auth.Application.Features;

/// <summary>
/// Query to retrieve a paginated list of users with their roles and permissions.
/// </summary>
public sealed record GetUsersQuery(int Page = 1, int PageSize = 20);

/// <summary>
/// Response containing paginated user data.
/// </summary>
public sealed record GetUsersResponse(List<UserDto> Users, int TotalCount, int Page, int PageSize);

/// <summary>
/// Wolverine handler for <see cref="GetUsersQuery"/>.
/// Replaces the admin GetUsers endpoint logic from UserService.
/// </summary>
public static class GetUsersHandler
{
    public static async Task<GetUsersResponse> Handle(
        GetUsersQuery query,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var (users, totalCount) = await userRepository.GetPagedAsync(page, pageSize, cancellationToken);

        var userDtos = users.Select(u => new UserDto(
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
                .ToList())).ToList();

        return new GetUsersResponse(userDtos, totalCount, page, pageSize);
    }
}
