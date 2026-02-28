using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class GetCurrentUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    private GetCurrentUserHandler CreateSut() => new(_userRepository);

    [Fact]
    public async Task Handle_ExistingUser_ReturnsUserDto()
    {
        // Arrange
        var permission = TestHelpers.CreatePermission(PermissionModule.Auth, PermissionAction.View);
        var role = TestHelpers.CreateRole("Admin", "Admin role", true, permission);
        var user = TestHelpers.CreateFullyWiredUser("admin@test.com", "Admin User", "hash", true, (role, [permission]));

        _userRepository.GetByIdWithRolesAndPermissionsAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetCurrentUserQuery(user.Id);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("admin@test.com");
        result.Value.FullName.Should().Be("Admin User");
        result.Value.Roles.Should().Contain("Admin");
        result.Value.Permissions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdWithRolesAndPermissionsAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var query = new GetCurrentUserQuery(userId);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
