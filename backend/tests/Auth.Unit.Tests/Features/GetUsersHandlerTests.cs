using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class GetUsersHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Handle_ReturnsPagedUsers()
    {
        // Arrange
        var permission = TestHelpers.CreatePermission(PermissionModule.Auth, PermissionAction.View);
        var role = TestHelpers.CreateRole("Doctor", "Doctor role", false, permission);
        var user1 = TestHelpers.CreateFullyWiredUser("user1@test.com", "User One", "hash", true, (role, [permission]));
        var user2 = TestHelpers.CreateFullyWiredUser("user2@test.com", "User Two", "hash", true, (role, [permission]));

        _userRepository.GetPagedAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(([user1, user2], 2));

        var query = new GetUsersQuery(Page: 1, PageSize: 20);

        // Act
        var result = await GetUsersHandler.Handle(query, _userRepository, CancellationToken.None);

        // Assert
        result.Users.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Users[0].Email.Should().Be("user1@test.com");
    }

    [Fact]
    public async Task Handle_ClampsPageSize()
    {
        // Arrange
        _userRepository.GetPagedAsync(1, 100, Arg.Any<CancellationToken>())
            .Returns((new List<User>(), 0));

        var query = new GetUsersQuery(Page: 1, PageSize: 500);

        // Act
        var result = await GetUsersHandler.Handle(query, _userRepository, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(100);
        await _userRepository.Received(1).GetPagedAsync(1, 100, Arg.Any<CancellationToken>());
    }
}
