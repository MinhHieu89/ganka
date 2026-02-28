using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class UpdateUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesAndSaves()
    {
        // Arrange
        var existingRole = TestHelpers.CreateRole("OldRole");
        var newRole = TestHelpers.CreateRole("NewRole");
        var user = TestHelpers.CreateUser("user@test.com");
        user.AssignRole(existingRole);

        var command = new UpdateUserCommand(user.Id, "Updated Name", true, [newRole.Id]);

        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _roleRepository.GetByIdAsync(newRole.Id, Arg.Any<CancellationToken>()).Returns(newRole);

        // Act
        var result = await UpdateUserHandler.Handle(
            command, _userRepository, _roleRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new UpdateUserCommand(userId, "Name", true, [Guid.NewGuid()]);

        // Act
        var result = await UpdateUserHandler.Handle(
            command, _userRepository, _roleRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_DeactivateUser_CallsDeactivate()
    {
        // Arrange
        var user = TestHelpers.CreateUser("user@test.com", isActive: true);
        var command = new UpdateUserCommand(user.Id, "Name", false, []);

        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await UpdateUserHandler.Handle(
            command, _userRepository, _roleRepository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }
}
