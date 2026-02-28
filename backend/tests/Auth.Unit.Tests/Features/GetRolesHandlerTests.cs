using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class GetRolesHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();

    [Fact]
    public async Task Handle_ReturnsAllRoles()
    {
        // Arrange
        var permission = TestHelpers.CreatePermission(PermissionModule.Patient, PermissionAction.View, "View patients");
        var role = TestHelpers.CreateRole("Doctor", "Doctor role", true, permission);
        TestHelpers.SetRolePermissionNavigation(role, permission);

        _roleRepository.GetAllWithPermissionsAsync(Arg.Any<CancellationToken>())
            .Returns([role]);

        var query = new GetRolesQuery();

        // Act
        var result = await GetRolesHandler.Handle(query, _roleRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Doctor");
        result[0].IsSystem.Should().BeTrue();
        result[0].Permissions.Should().HaveCount(1);
        result[0].Permissions[0].Module.Should().Be("Patient");
        result[0].Permissions[0].Action.Should().Be("View");
    }

    [Fact]
    public async Task Handle_EmptyRoles_ReturnsEmptyList()
    {
        // Arrange
        _roleRepository.GetAllWithPermissionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Role>());

        var query = new GetRolesQuery();

        // Act
        var result = await GetRolesHandler.Handle(query, _roleRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
