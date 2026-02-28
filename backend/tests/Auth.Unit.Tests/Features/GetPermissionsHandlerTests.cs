using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class GetPermissionsHandlerTests
{
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();

    [Fact]
    public async Task Handle_ReturnsAllPermissionsGroupedByModule()
    {
        // Arrange
        var perm1 = TestHelpers.CreatePermission(PermissionModule.Patient, PermissionAction.View, "View patients");
        var perm2 = TestHelpers.CreatePermission(PermissionModule.Patient, PermissionAction.Create, "Create patients");
        var perm3 = TestHelpers.CreatePermission(PermissionModule.Auth, PermissionAction.Manage, "Manage auth");

        _permissionRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([perm1, perm2, perm3]);

        var query = new GetPermissionsQuery();

        // Act
        var result = await GetPermissionsHandler.Handle(query, _permissionRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2); // Patient group + Auth group
        var patientGroup = result.First(g => g.Module == "Patient");
        patientGroup.Permissions.Should().HaveCount(2);

        var authGroup = result.First(g => g.Module == "Auth");
        authGroup.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyPermissions_ReturnsEmptyList()
    {
        // Arrange
        _permissionRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Permission>());

        var query = new GetPermissionsQuery();

        // Act
        var result = await GetPermissionsHandler.Handle(query, _permissionRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
