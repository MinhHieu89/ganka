using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class UpdateRolePermissionsHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ValidPermissions_UpdatesAndSaves()
    {
        // Arrange
        var role = TestHelpers.CreateRole("CustomRole");
        var perm1 = TestHelpers.CreatePermission(PermissionModule.Patient, PermissionAction.View);
        var perm2 = TestHelpers.CreatePermission(PermissionModule.Patient, PermissionAction.Create);
        var command = new UpdateRolePermissionsCommand(role.Id, [perm1.Id, perm2.Id]);

        _roleRepository.GetByIdWithPermissionsAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([perm1, perm2]);

        // Act
        var result = await UpdateRolePermissionsHandler.Handle(
            command, _roleRepository, _permissionRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.RolePermissions.Should().HaveCount(2);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentRole_ReturnsFailure()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var command = new UpdateRolePermissionsCommand(roleId, [Guid.NewGuid()]);

        // Act
        var result = await UpdateRolePermissionsHandler.Handle(
            command, _roleRepository, _permissionRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
