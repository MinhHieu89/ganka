using Auth.Contracts.Dtos;
using Shared.Domain;

namespace Auth.Application.Services;

/// <summary>
/// User management service for admin operations.
/// </summary>
public interface IUserService
{
    Task<Result<(List<UserDto> Users, int TotalCount)>> GetUsersAsync(int page, int pageSize);
    Task<Result<Guid>> CreateUserAsync(CreateUserCommand command);
    Task<Result> UpdateUserAsync(Guid userId, UpdateUserCommand command);
    Task<Result> AssignRolesAsync(Guid userId, AssignRolesCommand command);
}
