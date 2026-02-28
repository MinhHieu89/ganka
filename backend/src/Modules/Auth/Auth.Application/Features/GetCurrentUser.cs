using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Shared.Domain;

namespace Auth.Application.Features;

// --- Query record ---
public sealed record GetCurrentUserQuery(Guid UserId);

// --- Handler ---
public sealed class GetCurrentUserHandler
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery query)
    {
        var user = await _userRepository.GetByIdWithRolesAndPermissionsAsync(query.UserId);

        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("User", query.UserId));

        var permissions = user.GetEffectivePermissions()
            .Select(p => p.ToString())
            .ToList();

        return new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PreferredLanguage,
            user.IsActive,
            user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            permissions);
    }
}
